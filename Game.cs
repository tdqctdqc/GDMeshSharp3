
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Game : Node
{
	public static Game I { get; private set; }
	public Logger Logger { get; private set; }
	public RandomNumberGenerator Random = new RandomNumberGenerator();
	private ISession _session;

	public Client Client => _session.Client;
	public override void _Ready()
	{
		if (I != null)
		{
			throw new Exception();
		}
		I = this;
		Logger = new Logger();
		Assets.Setup();
		StartMainMenuSession();
		
	}
	public void StartMainMenuSession()
	{
		AddChild(new MainMenu());
	}
	public void StartClientSession()
	{
		var session = GameSession.StartAsRemote();
		SetSession(session);
	}
	public void StartHostSession()
	{
		var session = GameSession.StartAsGenerator();
		SetSession(session);
	}
	public void LoadHostSession(Data data)
	{
		var session = GameSession.StartAsLoad(data);
		SetSession(session);
	}
	private void SetSession(Node session)
	{
		if(_session != null) RemoveChild((Node) _session);
		_session?.QueueFree();
		session.Name = "Session";
		_session = (ISession)session;
		AddChild(session);
	}
}
