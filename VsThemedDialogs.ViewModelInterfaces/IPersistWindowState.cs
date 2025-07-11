namespace VsThemedDialogs
{
    public interface IPersistWindowState
    {
        WindowPersistence GetState();

        void SetState(WindowPersistence persistence);
    }

    public class WindowPersistence
    {
        public double Left { get; set; }

        public double Top { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
