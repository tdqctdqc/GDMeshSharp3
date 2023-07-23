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
    private bool _justGenned = false;
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

    public void TryGenerate()
    {
        _generating = true;
        GD.Print("TRYING GEN");     

        try
        {       
            Generate();
        }
        catch (Exception e)
        {
            GD.Print("RETRYING GEN");
            RetryGen();
        }
        _generating = false;
        _justGenned = true;
    }
    public void Generate()
    {
        Game.I.Random.Seed = (ulong) GenMultiSettings.PlanetSettings.Seed.Value;
        if (Generated)
        {
            Reset();
        }
        WorldGen.Generate();
        Generated = true;
    }

    private int _tries = 0;
    private void RetryGen()
    {
        Reset();
        try
        {
            _tries++;
            Generate();
        }
        catch (Exception e)
        {
            GD.Print("RETRYING GEN FAILED");
            if (_tries > 10) throw e;
            else
            {
                GD.Print("RE RETRYING GEN");

                RetryGen();
            }
        }
    }

    private void Reset()
    {
        Client.Free();
        this.ClearChildren();
        Client = null;
        Game.I.SetSerializer();
        Server = new DummyServer();
        Data = new GenData(GenMultiSettings);
        _key = new ClientWriteKey(Data, this);
        WorldGen = new WorldGenerator(this, Data);
        Client = new GeneratorClient();
        Client.Setup(this);
        AddChild(Client);
    }
    public override void _Process(double delta)
    {
        if (_justGenned)
        {
            Client.StartMapGraphics();
            _justGenned = false;
        }
        Client?.Process((float)delta);
    }
}