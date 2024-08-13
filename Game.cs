
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
public partial class Game : Node
{
	public static Game I { get; private set; }
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
		Assets.Setup();
		StartMainMenuSession();
		// PathFinderTest.Test();
		
		// var r = ExcelDataReader.ExcelReaderFactory
		// 	.CreateReader();
		// RunsTest();
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



	private void RunsTest()
	{
		var test = new List<int> { 1, 1, 1, 2, 2, 3, 3, 3, 1 };
		var runs = new HashSet<List<int>>();
		test.DoForRuns(i => i,
			l => runs.Add(l),
			(i,j) => false);
		foreach (var run in runs)
		{
			GD.Print("RUN");
			foreach (var i in run)
			{
				GD.Print(i);
			}
		}
	}
}
