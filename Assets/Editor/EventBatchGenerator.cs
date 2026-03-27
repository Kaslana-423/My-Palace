using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EventBatchGenerator : EditorWindow
{
    private TextAsset dataFile;
    private string outputFolder = "Assets/RandomEvents"; // SO输出路径

    [MenuItem("Tools/从文本批量生成事件SO (TSV)")]
    public static void ShowWindow()
    {
        GetWindow<EventBatchGenerator>("事件生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("数据驱动 - 批量生成 GameEvent", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 接收 TextAsset 拖拽
        dataFile = (TextAsset)EditorGUILayout.ObjectField("拖入数据文本 (TXT):", dataFile, typeof(TextAsset), false);

        // 设置输出路径
        outputFolder = EditorGUILayout.TextField("SO 输出文件夹:", outputFolder);

        EditorGUILayout.Space();
        
        if (GUILayout.Button("开始批量生成", GUILayout.Height(40))) GenerateEvents();
    }

    private void GenerateEvents()
    {
        if (dataFile == null) return;

        // 检查并创建输出文件夹
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string pFolder = Path.GetDirectoryName(outputFolder).Replace("\\", "/");
            string nFolder = Path.GetFileName(outputFolder);
            AssetDatabase.CreateFolder(pFolder, nFolder);
        }

        // 按行分割文本
        string[] lines = dataFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        GameEvent currentEvent = null;
        int generatedCount = 0;

        for (int i = 1; i < lines.Length; i++) 
        {
            string[] cols = lines[i].Split('\t');
            if (cols.Length < 10) continue; // 至少保证有基础数值项才解析

            string eTitle = cols[0].Trim();
            string eDesc = cols[1].Trim();
            int weight = ParseInt(cols[2], 100);

            string oText = cols[3].Trim();
            string oDesc = cols[4].Trim();

            // 立即生效数值
            int cCoin = ParseInt(cols[5]), cMat = ParseInt(cols[6]), cPop = ParseInt(cols[7]), cPros = ParseInt(cols[8]), cAng = ParseInt(cols[9]);

            if (!string.IsNullOrEmpty(eTitle))
            {
                currentEvent = ScriptableObject.CreateInstance<GameEvent>();
                currentEvent.eventTitle = eTitle;
                currentEvent.eventDescription = eDesc;
                currentEvent.weight = weight;
                currentEvent.options = new List<EventOption>();

                string safeName = eTitle.Replace(":", "_").Replace("/", "_").Replace("\\", "_");
                string path = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/Event_{safeName}.asset");
                AssetDatabase.CreateAsset(currentEvent, path);
                generatedCount++;
            }

            if (currentEvent != null && !string.IsNullOrEmpty(oText))
            {
                EventOption opt = new EventOption
                {
                    optionText = oText,
                    optionDescription = oDesc,
                    coinChange = cCoin, materialChange = cMat, populationChange = cPop, prosperityChange = cPros, personAngerChange = cAng,
                    buffResult = new GlobalBuff()
                };

                // ---- 解析后排的 Buff 字段 (可选) ----
                // 如果 Excel 列数达到了 22 列，说明有配 Buff 数据
                if (cols.Length >= 22)
                {
                    opt.hasBuff = ParseInt(cols[10], 0) == 1; // 填 1 就是 true
                    if (opt.hasBuff)
                    {
                        opt.buffResult.remainRounds = ParseInt(cols[11], 0);

                        opt.buffResult.coinMult = ParseFloat(cols[12], 1f);
                        opt.buffResult.coinAdd = ParseInt(cols[13], 0);

                        opt.buffResult.materialMult = ParseFloat(cols[14], 1f);
                        opt.buffResult.materialAdd = ParseInt(cols[15], 0);

                        opt.buffResult.popMult = ParseFloat(cols[16], 1f);
                        opt.buffResult.popAdd = ParseInt(cols[17], 0);

                        opt.buffResult.prosMult = ParseFloat(cols[18], 1f);
                        opt.buffResult.prosAdd = ParseInt(cols[19], 0);

                        opt.buffResult.angerMult = ParseFloat(cols[20], 1f);
                        opt.buffResult.angerAdd = ParseInt(cols[21], 0);
                    }
                }
                
                currentEvent.options.Add(opt);
                EditorUtility.SetDirty(currentEvent);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", $"成功生成 {generatedCount} 个事件SO。", "确定");
    }

    private int ParseInt(string val, int def = 0) { return int.TryParse(val.Trim(), out int res) ? res : def; }
    private float ParseFloat(string val, float def = 1f) { return float.TryParse(val.Trim(), out float res) ? res : def; }
}