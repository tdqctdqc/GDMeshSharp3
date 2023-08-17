using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class GeneratorUi : Node, IClientComponent
{
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    private WorldGenLogic _logic;
    private Label _progress;
    private GameSession _session;
    
    public GeneratorSettingsWindow GenSettingsWindow { get; private set; }
    public static GeneratorUi Construct(Client client, GameSession session, WorldGenLogic wrapper)
    {
        var ui = new GeneratorUi();
        ui.Setup(client, session, wrapper);
        return ui;
    }

    protected GeneratorUi() : base()
    {
        
    }
    public void Setup(Client client, GameSession session, WorldGenLogic wrapper)
    {
        _session = session;
        _logic = wrapper;
        var uiFrame = client.GetComponent<UiFrame>();

        var topBar = new HBoxContainer();
        uiFrame.AddTopBar(topBar);

        topBar.AddButton("Generate", PressedGenerate);
        topBar.AddButton("Done", () =>
        {
            if(wrapper.Succeeded)
            {
                _session.GeneratorToGameTransition();
            }
        });
        topBar.AddWindowButton<GeneratorSettingsWindow>("Gen Settings");
        
        var genSettingsWindow = GeneratorSettingsWindow.Get(wrapper.Data.GenMultiSettings);
        var windows = client.GetComponent<WindowManager>();
        windows.AddWindow(genSettingsWindow);

        _progress = new Label();
        _progress.Text = "Progress";
        uiFrame.LeftSidebar.AddChild(_progress);
        
        Disconnect += () =>
        {
            windows.RemoveWindow(genSettingsWindow);
            topBar.QueueFree();
            _progress.QueueFree();
        };
    }
    
    public void GoToGameSession()
    {
    }
    private void PressedGenerate()
    {
        if (_logic.Generating) return;
        _logic.TryGenerate();
        try
        {
            // await Task.Run(_logic.TryGenerate);
            
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
                    throw;
                }
            }
        }
    }

    
    private void DisplayException(DisplayableException d)
    {
        var display = new Node2D();
        GD.Print(d.StackTrace);
                
        var graphic = d.GetGraphic();
        display.AddChild(graphic);
        Game.I.Client.GraphicsLayer.AddChild(display);
    }
    
    Node IClientComponent.Node => this;
}