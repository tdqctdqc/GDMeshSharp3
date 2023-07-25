using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class GeneratorUi : Ui
{
    private GeneratorSession _session;
    private bool _generating;
    private Label _progress;
    public TooltipManager TooltipManager { get; private set; }
    public MapGraphicsOptions MapGraphicsOptions { get; private set; }

    // public GeneratorSettingsWindow GenSettingsWindow { get; private set; }
    public static GeneratorUi Construct(GeneratorClient client, GeneratorSession session)
    {
        var ui = new GeneratorUi(client);
        ui.Setup(client, session);
        return ui;
    }
    private GeneratorUi() : base() 
    {
    }

    protected GeneratorUi(IClient client) : base()
    {
        
    }
    public void Setup(GeneratorClient client, GeneratorSession session)
    {
        Setup(client);
        _session = session;
        var topBar = ButtonBarToken.Create<HBoxContainer>();
        topBar.AddButton("Generate", PressedGenerate);
        topBar.AddButton("Done", GoToGameSession);
        topBar.AddWindowButton<GeneratorSettingsWindow>("Gen Settings");
        topBar.AddWindowButton<LoggerWindow>("Logger");
        topBar.AddButton("Test Serialization", () => session.Data.Serializer.Test(session.Data));
        topBar.AddButton("Save", () => Saver.Save(session.Data));
        topBar.AddButton("Load", () => Saver.Load());
        
        
        AddChild(topBar.Container); 

        var genSettingsWindow = GeneratorSettingsWindow.Get(_session.GenMultiSettings);
        AddWindow(genSettingsWindow);

        var loggerWindow = LoggerWindow.Get();
        AddWindow(loggerWindow);
        
        var sideBar = ButtonBarToken.Create<VBoxContainer>();
        AddChild(sideBar.Container);
        _progress = new Label();
        _progress.Text = "Progress";
        sideBar.Container.AddChild(_progress);
        MapGraphicsOptions = new MapGraphicsOptions();
        sideBar.Container.Position = Vector2.Down * 50f;
        sideBar.Container.AddChild(MapGraphicsOptions);
        AddWindow(new RegimeOverviewWindow());
        
        TooltipManager = new TooltipManager(session.Data);
        AddChild(TooltipManager);
    }
    public void Process(float delta, ICameraController cam)
    {
        TooltipManager.Process(delta, cam.GetMousePosInMapSpace());
    }
    public void GoToGameSession()
    {
        if (_session.Generated)
        {
            Game.I.StartHostSession(_session.Data);
        }
    }
    private async void PressedGenerate()
    {
        if (_generating) return;
        _generating = true;
        await Task.Run(_session.TryGenerate); 

        try
        {
        }
        catch (Exception e)
        {
            if (e is DisplayableException d)
            {
                DisplayException(d);
            }
            else
            {
                if (e is AggregateException a
                    && a.InnerExceptions.FirstOrDefault(i => i is DisplayableException) is DisplayableException da)
                {
                    DisplayException(da);
                }
                else
                {
                    GD.Print(e.Message);
                    GD.Print(e.StackTrace);
                    throw e;
                }
            }
        }
        
        _generating = false;
    }

    private void DisplayException(DisplayableException d)
    {
        var display = new Node2D();
        AddChild(display);
        GD.Print(d.StackTrace);
                
        var graphic = d.GetGraphic();
        display.AddChild(graphic);
        var cam = new DebugCameraController(graphic);

        display.AddChild(cam);
    }
}