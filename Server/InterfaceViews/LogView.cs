using Terminal.Gui;

namespace Server.InterfaceViews;

public sealed class LogView : View {
    //I  failed to make it work with ListView (props must be initialized exception)
    // so I replaced it with textView

    // public ListView Data = new ListView() {
    // X = 1, Y = 1,
    // Width = 60, Height = 30, AllowsMarking = true,
    // };

    private readonly TextView logs = new() { X = 1, Y = 2, AutoSize = true };
    private int lastLogIndex;

    public LogView() {
        this.Width = Dim.Fill();
        logs.Width = Dim.Fill();
        logs.Height = Dim.Fill();

        Add(logs);
    }

    public void Update() {
        if (lastLogIndex == Program.Logs.Count)
            return;
        logs.Text += string.Join("\n", Program.Logs.Slice(
            lastLogIndex, Program.Logs.Count - lastLogIndex)) + "\n";
        lastLogIndex = Program.Logs.Count;
    }
}