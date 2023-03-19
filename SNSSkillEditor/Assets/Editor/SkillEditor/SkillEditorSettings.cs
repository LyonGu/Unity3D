namespace SkillEditor
{
    public static class SkillEditorSettings
    {
        //场景路径
        public static string EditorScenePath = "Assets/Editor/SkillEditor/SkillEditor.unity";
        //导出和读取的json路径
        public static string SkillJsonPath = "Assets/StreamingAssets/asset_base/skillclient/skillclient.json";
        //Timeline窗口默认显示的时间范围
        public static float DefaultTimeRange = 2f;
        //预警圈prefab路径
        public static string AlertEffectPrefabPath = "Assets/Arts/Effects/BattleSkill/alert.prefab";
    }
}