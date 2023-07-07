using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class GeneratorSession : Node, IDataSession
{
    Data IDataSession.Data => Data;
    public GenData Data { get; private set; }
    IClient ISession.Client => Client;
    public GeneratorClient Client { get; private set; }
    public WorldGenerator WorldGen { get; private set; }
    public bool Generated { get; private set; } = false;
    private bool _generating = false;
    public IServer Server { get; private set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }
    private ClientWriteKey _key;

    public GeneratorSession()
    {
        GenMultiSettings = new GenerationMultiSettings();
    }
    public void Setup()
    {
        Server = new DummyServer();
        Data = new GenData(GenMultiSettings);
        _key = new ClientWriteKey(Data, this);
        WorldGen = new WorldGenerator(this, Data);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }

    private int _tries = 0;
    public void Generate()
    {
        _generating = true;
        try
        {
            Game.I.Random.Seed = (ulong) GenMultiSettings.PlanetSettings.Seed.Value;
            _tries++;
            if (Generated)
            {
                Reset();
            }
            WorldGen.Generate();
            Generated = true;
            _generating = false;
            Client.Graphics.Setup(_key);
        }
        catch (Exception e)
        {
            if (_tries > 10)
            {
                GD.Print("Generation failed too many times");
                throw e;
            }
            else
            {
                GD.Print("Generation failed, retrying");
                Reset();
                Generate();
            }
        }
    }

    private void Reset()
    {
        this.ClearChildren();
        Client = null;
        Game.I.SetSerializer();
        Server = new DummyServer();
        Data = new GenData(GenMultiSettings);
        WorldGen = new WorldGenerator(this, Data);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }
    public override void _Process(double delta)
    {
        if(_generating == false) Client?.Process((float)delta, false);
    }
}