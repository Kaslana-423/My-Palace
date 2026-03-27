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
        EditorGUILayout.HelpBox("提示：\n直接从 Excel 复制表格内容，粘贴到 TXT 文件中即可（自动识别制表符 \\t）。\n第一行将被视为表头跳过。", MessageType.Info);

        if (GUILayout.Button("开始批量生成", GUILayout.Height(40)))
        {
            GenerateEvents();
        }
    }

    private void GenerateEvents()
    {
        if (dataFile == null)
        {
            EditorUtility.DisplayDialog("错误", "请先拖入数据 TXT 文件！", "确定");
            return;
        }

        // 检查并创建输出文件夹
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parentFolder = Path.GetDirectoryName(outputFolder).Replace("\\", "/");
            string newFolder = Path.GetFileName(outputFolder);
            if (AssetDatabase.IsValidFolder(parentFolder))
            {
                AssetDatabase.CreateFolder(parentFolder, newFolder);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "目标路径无效，请手动创建该文件夹。默认建议：Assets/GameEvents", "确定");
                return;
            }
        }

        // 按行分割文本
        string[] lines = dataFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        GameEvent currentEvent = null;
        int generatedCount = 0;

        for (int i = 1; i < lines.Length; i++) // 从 i=1 开始，跳过第一行的表头
        {
            // 这是从 Excel 复制的 TXT 默认的分隔符：制表符 \t
            string[] columns = lines[i].Split('\t');

            // 如果列数不够 9 列，忽略这一行
            if (columns.Length < 9) continue;

            string eventTitle = columns[0].Trim();
            string eventDesc = columns[1].Trim();

            string optText = columns[2].Trim();
            string optDesc = columns[3].Trim();

            int coin = ParseInt(columns[4]);
            int mat = ParseInt(columns[5]);
            int pop = ParseInt(columns[6]);
            int pros = ParseInt(columns[7]);
            int anger = ParseInt(columns[8]);

            // 如果读取到了非空的“事件标题”，说明是一个新事件，需要创建新的 SO
            if (!string.IsNullOrEmpty(eventTitle))
            {
                currentEvent = ScriptableObject.CreateInstance<GameEvent>();
                currentEvent.eventTitle = eventTitle;
                currentEvent.eventDescription = eventDesc;
                currentEvent.options = new List<EventOption>();

                // 安全的文件名，去除特殊字符
                string safeFileName = eventTitle.Replace(":", "_").Replace("/", "_").Replace("\\", "_");
                string assetPath = $"{outputFolder}/Event_{safeFileName}.asset";

                // 解决重名覆盖问题
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                AssetDatabase.CreateAsset(currentEvent, assetPath);
                generatedCount++;
            }

            // 给当前事件添加选项内容
            if (currentEvent != null && !string.IsNullOrEmpty(optText))
            {
                EventOption newOption = new EventOption
                {
                    optionText = optText,
                    optionDescription = optDesc,
                    coinChange = coin,
                    materialChange = mat,
                    populationChange = pop,
                    prosperityChange = pros,
                    personAngerChange = anger
                };
                currentEvent.options.Add(newOption);

                // 标记该 SO 已经被修改并需要保存
                EditorUtility.SetDirty(currentEvent);
            }
        }

        // 统一保存所有新创建的资产
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成", $"批量生成已完成！\n共生成了 {generatedCount} 个事件 SO 文件。\n请前往 {outputFolder} 查看。", "太棒了");
    }

    // 辅助解析：即使 Excel 里填空了，也默认转为 0
    private int ParseInt(string value)
    {
        if (int.TryParse(value.Trim(), out int result))
        {
            return result;
        }
        return 0;
    }
}