using System.Threading.Tasks;

namespace LitEngine.DownLoad
{
    public interface IDownLoad : System.IDisposable
    {
        string Key { get; }
        bool IsDone { get; }
        bool IsCompleteDownLoad { get; } //成功下载
        void StartAsync();
        void Update();
        void CallComplete();
    }
}