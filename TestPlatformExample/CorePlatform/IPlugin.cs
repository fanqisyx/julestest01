namespace CorePlatform
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void Load();
        void RunTest(Action<string> logCallback);
        void Unload();
    }
}
