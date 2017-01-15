using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.HyperNeat;
using System.Collections.Generic;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System;
using System.Xml;
using System.IO;
using SharpNeat.Network;
using SharpNeat.Decoders.HyperNeat;

public class Optimizer : MonoBehaviour {

	public enum RunType
	{
		TEST,
		EVOLVE
	};

	public RunType runType = RunType.EVOLVE;


    int NUM_INPUTS;
    int NUM_OUTPUTS;

	public float evoSpeed = 1;
	public float runBestSpeed = 1;

    public int Trials;
    public float TrialDuration;
	public int StoppingGeneration;
	public int NumRuns;
    bool EARunning;
    string popFileSavePath, champFileSavePath, superChampFileSavePath;

	public float Test_trialDuration = 0;
	private float Test_trialDurationTimer = 0;
	public float Test_carsPerStartPoint= 0;
	public float Test_numTrials= 20;
	private float Test_currentTrial= 1;

    float normalFixedDeltaTime;

    SimpleExperiment experiment;
    static NeatEvolutionAlgorithm<NeatGenome> _ea;

    public GameObject Unit;

    Dictionary<IBlackBox, UnitController> ControllerMap = new Dictionary<IBlackBox, UnitController>();
    private DateTime startTime;
    private float timeLeft;
    private float accum;
    private int frames;
    private float updateInterval = 12;

    private uint Generation;
    private double Fitness;
	SimulationController simController = null;
	FileBrowser browser;
	string mode = "";

	// Use this for initialization
	void Start ()
	{
        if (Config.NEAT)
        {
            NUM_INPUTS = 4; //Sensors
            NUM_OUTPUTS = 1; //Speed
        }
        else if (Config.HyperNEAT)
        {
            NUM_INPUTS = 4;  //Two substrate nodes (x1,y1,x2,y2) 
            NUM_OUTPUTS = 2; //Weight strength + bias
        }


		simController = GameObject.Find ("SimulationController").GetComponent<SimulationController> ();
		simController.setup ();
		champFileSavePath = string.Format ("/{0}.champ.xml", "NEAT_Controller");
		popFileSavePath = string.Format ("/{0}.pop.xml", "NEAT_Controller");
		superChampFileSavePath = string.Format ("/{0}.SUPERchamp.xml", "NEAT_Controller");
		normalFixedDeltaTime = Time.fixedDeltaTime;


		Utility.DebugLog = true;
		experiment = new SimpleExperiment ();
		XmlDocument xmlConfig = new XmlDocument ();
		TextAsset textAsset = (TextAsset)Resources.Load ("experiment.config");
		xmlConfig.LoadXml (textAsset.text);
		experiment.SetOptimizer (this);

		experiment.Initialize ("NEAT Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

		if (runType == RunType.TEST)
		{
			test_currentGroupController = testController (test_currentRun);
		}

		browser = new FileBrowser (string.Format (UnityEngine.Application.dataPath + "/Resources/"), 1, new Rect(new Vector2(0,0), new Vector2(300,300)));

		if (Config.NEAT)
		{
			mode = "NEAT";
		}
		else if (Config.HyperNEAT)
		{
			mode = "HYPERNEAT";
		}

		}

	private GameObject test_currentGroupController;
	public int test_currentRun = 1;
	public int test_numRuns = 20;

    // Update is called once per frame
    void Update()
    {
      //  evaluationStartTime += Time.deltaTime;

        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeLeft <= 0.0)
        {
            var fps = accum / frames;
            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
            //   print("FPS: " + fps);
            if (fps < 10)
            {
                Time.timeScale = Time.timeScale - 1;
                print("Lowering time scale to " + Time.timeScale);
            }
        }

		if (runType == RunType.EVOLVE) {
			
			if (Generation >= StoppingGeneration) {
				StopEA ();
			}

			if (guiForm != null) {
				guiForm.RefreshView ();
			}

			if (guiForm2 != null) {
				guiForm2.RefreshView ();
			}
		}
		else
		{
			Test_trialDurationTimer += Time.deltaTime;
			if(Test_trialDurationTimer >= Test_trialDuration)
			{
				Test_trialDurationTimer = 0;
				test_currentGroupController.GetComponent<NEAT_GroupController> ().record ();
				GameObject.DestroyImmediate (test_currentGroupController);
				print ("completed evaluating champ of run " + test_currentRun + "for trial "+Test_currentTrial);
				Test_currentTrial++;
				if (Test_currentTrial > Test_numTrials)
				{
					test_currentRun++;
					Test_currentTrial = 1;
					if (test_currentRun > test_numRuns) {
						#if UNITY_EDITOR
						UnityEditor.EditorApplication.isPlaying = false;
						#else
					Application.Quit();
						#endif
					}
				}
			
				test_currentGroupController = testController (test_currentRun);

			}
		}


    }

    public void StartEA()
    {        
        Utility.DebugLog = true;
        Utility.Log("Starting NEAT Training");
        _ea = experiment.CreateEvolutionAlgorithm();
        startTime = DateTime.Now;

        _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);
        _ea.PausedEvent += new EventHandler(ea_PauseEvent);

     //   Time.fixedDeltaTime = 0.045f;
        Time.timeScale = evoSpeed;       
        _ea.StartContinue();
        EARunning = true;
    }

    void ea_UpdateEvent(object sender, EventArgs e)
    {
        Utility.Log(string.Format("gen={0:N0} bestFitness={1:N6}",
            _ea.CurrentGeneration, _ea.Statistics._maxFitness));

        Fitness = _ea.Statistics._maxFitness;
        Generation = _ea.CurrentGeneration;
      

    //    Utility.Log(string.Format("Moving average: {0}, N: {1}", _ea.Statistics._bestFitnessMA.Mean, _ea.Statistics._bestFitnessMA.Length));

    
    }

	public GameObject testController(int path)
	{
		Time.timeScale = runBestSpeed;

		NeatGenome genome = null;

		try
		{
			if (Config.NEAT)
			{
				using (XmlReader xr = XmlReader.Create(Application.dataPath+"/saves/"+test_currentRun+champFileSavePath))
					genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, (NeatGenomeFactory)experiment.CreateGenomeFactory())[0];
			}
			else 
			{
				using (XmlReader xr = XmlReader.Create(Application.dataPath+"/saves/"+test_currentRun+champFileSavePath))
					genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, true, (CppnGenomeFactory)experiment.CreateGenomeFactory())[0];
			}

		}
		catch (Exception e1)
		{
			print("Error loading genome from save file. ("+Application.dataPath+"/saves/"+test_currentRun+champFileSavePath+")\nLoading aborted.\n"
				+ e1.Message);
			return null;
		}

		// Get a genome decoder that can convert genomes to phenomes.
		var genomeDecoder = experiment.CreateGenomeDecoder();

		// Decode the genome into a phenome (neural network).
		var phenome = genomeDecoder.Decode(genome);



		//guiForm = new SharpNeatGUI.GenomeForm ("GUI", new SharpNeat.Domains.NeatGenomeView(), genome);


		GameObject groupController = Instantiate(Unit, Vector3.zero, Quaternion.identity) as GameObject;
		UnitController controller = groupController.GetComponent<UnitController>();
		((NEAT_GroupController)controller).setPathManager (simController.getCurrentTrack ().GetComponentInChildren<PathManager> ());
		((NEAT_GroupController)controller).optimizer = this;

		if (!ControllerMap.ContainsKey (phenome))
		{
			ControllerMap.Add (phenome, controller);
		}
		else
		{
			ControllerMap [phenome] = controller;
		}

		controller.Activate(phenome);

		return groupController;
	}

    void ea_PauseEvent(object sender, EventArgs e)
    {
        Time.timeScale = 1;
        Utility.Log("Done ea'ing (and neat'ing)");

        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        // Save genomes to xml file.        
		DirectoryInfo dirInf = new DirectoryInfo(Application.dataPath+"/Resources/"+currentRun);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
		using (XmlWriter xw = XmlWriter.Create(Application.dataPath+"/Resources/"+currentRun+popFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _ea.GenomeList);
        }
        // Also save the best genome

		using (XmlWriter xw = XmlWriter.Create(Application.dataPath+"/Resources/"+currentRun+champFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
        }
        DateTime endTime = DateTime.Now;
        Utility.Log("Total time elapsed: " + (endTime - startTime));

		//System.IO.StreamReader stream = new System.IO.StreamReader(Application.dataPath+"/Resources/"+currentRun+popFileSavePath);

        EARunning = false;  

		if (currentRun < NumRuns)
		{
			superFitness = 0;
			currentRun++;
			StartEA ();
		}
        
    }

	public uint currentRun = 1;

    public void StopEA()
    {
        if (_ea != null && _ea.RunState == SharpNeat.Core.RunState.Running)
        {
            _ea.Stop();
        }
    }
		
    public void Evaluate(IBlackBox box)
    {
		GameObject groupController = Instantiate(Unit, Vector3.zero, Quaternion.identity) as GameObject;
		UnitController controller = groupController.GetComponent<UnitController>();
		((NEAT_GroupController)controller).setPathManager (simController.getCurrentTrack ().GetComponentInChildren<PathManager> ());
		if (!ControllerMap.ContainsKey (box))
		{
			ControllerMap.Add (box, controller);
		}
		else
		{
			ControllerMap [box] = controller;
		}

		controller.Activate(box);
    }

	string xmlPath = "";
    double superFitness = 0;
    public void StopEvaluation(IBlackBox box)
    {
		//save superChamp!
		if (_ea.CurrentChampGenome.EvaluationInfo.Fitness > superFitness)
		{
			superFitness = _ea.CurrentChampGenome.EvaluationInfo.Fitness;
			XmlWriterSettings _xwSettings = new XmlWriterSettings ();
			_xwSettings.Indent = true;
			// Save genomes to xml file.        
			DirectoryInfo dirInf = new DirectoryInfo (Application.dataPath+"/Resources/"+currentRun);
			if (!dirInf.Exists) {
				Debug.Log ("Creating subdirectory");
				dirInf.Create ();
			}
			using (XmlWriter xw = XmlWriter.Create (Application.dataPath+"/Resources/"+currentRun+superChampFileSavePath, _xwSettings)) {
				experiment.SavePopulation (xw, new NeatGenome[] { _ea.CurrentChampGenome });
			}
		}
		((NEAT_GroupController)ControllerMap [box]).cleanUp ();
    }

	SharpNeatGUI.GenomeForm guiForm;
	SharpNeatGUI.GenomeForm guiForm2;
    public void RunBest()
    {
		Time.timeScale = runBestSpeed;
		//Time.fixedDeltaTime = normalFixedDeltaTime * (1/runBestSpeed);
        NeatGenome genome = null;

        try
        {
			if (Config.NEAT)
			{
				print(xmlPath);
				using (XmlReader xr = XmlReader.Create(xmlPath))
                genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, (NeatGenomeFactory)experiment.CreateGenomeFactory())[0];
			}
			else 
			{
				using (XmlReader xr = XmlReader.Create(xmlPath))
					genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, true, (CppnGenomeFactory)experiment.CreateGenomeFactory())[0];
			}

		}
        catch (Exception e1)
        {
			print("Error loading genome from save file. ("+xmlPath+")\nLoading aborted.\n"
				+ e1.Message);
            return;
        }

        // Get a genome decoder that can convert genomes to phenomes.
        var genomeDecoder = experiment.CreateGenomeDecoder();
		HyperNeatDecoder hDecoder = (HyperNeatDecoder)experiment.CreateGenomeDecoder();
        // Decode the genome into a phenome (neural network).
        var phenome = genomeDecoder.Decode(genome);


		if (Config.NEAT)
			guiForm = new SharpNeatGUI.GenomeForm ("GUI", new SharpNeat.Domains.NeatGenomeView (), genome);
		else 
		{
			guiForm = new SharpNeatGUI.GenomeForm ("GUI", new SharpNeat.Domains.CppnGenomeView (DefaultActivationFunctionLibrary.CreateLibraryCppn ()), genome);
			guiForm2 = new SharpNeatGUI.GenomeForm ("GUI", new SharpNeat.Domains.NeatGenomeView (hDecoder.GetNetworkDefinition(genome)), genome);
		}

		GameObject groupController = Instantiate(Unit, Vector3.zero, Quaternion.identity) as GameObject;
		UnitController controller = groupController.GetComponent<UnitController>();
		((NEAT_GroupController)controller).setPathManager (simController.getCurrentTrack ().GetComponentInChildren<PathManager> ());
		((NEAT_GroupController)controller).optimizer = this;

		if (!ControllerMap.ContainsKey (phenome))
		{
			ControllerMap.Add (phenome, controller);
		}
		else
		{
			ControllerMap [phenome] = controller;
		}

        controller.Activate(phenome);
    }

    public float GetFitness(IBlackBox box)
    {
        if (ControllerMap.ContainsKey(box))
        {
            return ControllerMap[box].GetFitness();
        }
        return 0;
    }

    void OnGUI()
    {
		if (runType == RunType.EVOLVE)
		{
			if (GUI.Button (new Rect (10, 10, 100, 40), "Start EA")) {
				StartEA ();
			}
			if (GUI.Button (new Rect (10, 60, 100, 40), "Stop EA")) {
				StopEA ();
			}
			if (GUI.Button (new Rect (10, 110, 100, 40), "Open File")) {
				showFileBrowser ();
			}

			if (fileBrowserOpen) {
				if (browser.draw ()) {
					if (browser.outputFile != null) {
						xmlPath = browser.outputFile.ToString ();
						RunBest ();
						browser.outputFile = null;
						fileBrowserOpen = false;
					} else {
						fileBrowserOpen = false;
					}
				}
			}



			GUI.Button (new Rect (10, Screen.height - 140, 150, 90), string.Format ("Generation: {0}\nFitness: {1:0.00}\nSuperFitness: {3:0.00}\nCurrent Run: {5}\nTimeScale: {2}\n{4}", Generation, Fitness, Time.timeScale, superFitness, mode, currentRun));
		}

		}

	bool fileBrowserOpen = false;
	private void showFileBrowser()
	{
		fileBrowserOpen = true;
	}

}
