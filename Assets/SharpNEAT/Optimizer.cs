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

public class Optimizer : MonoBehaviour {

    int NUM_INPUTS;
    int NUM_OUTPUTS;

	public float evoSpeed = 1;
	public float runBestSpeed = 1;

    public int Trials;
    public float TrialDuration;
    public float StoppingFitness;
    bool EARunning;
    string popFileSavePath, champFileSavePath, superChampFileSavePath;

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


	// Use this for initialization
	void Start ()
	{
        if (Config.NEAT)
        {
            NUM_INPUTS = 10; //Sensors
            NUM_OUTPUTS = 1; //Speed
        }
        else if (Config.HyperNEAT)
        {
            NUM_INPUTS = 4;  //Two substrate nodes (x1,y1,x2,y2) 
            NUM_OUTPUTS = 2; //Weight stregnth + bias
        }

		System.Console.WriteLine("From optimizer.");
		simController = GameObject.Find ("SimulationController").GetComponent<SimulationController> ();

        Utility.DebugLog = true;
        experiment = new SimpleExperiment();
        XmlDocument xmlConfig = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("experiment.config");
        xmlConfig.LoadXml(textAsset.text);
        experiment.SetOptimizer(this);

        experiment.Initialize("NEAT Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

		champFileSavePath = string.Format(Application.dataPath+"/Resources/{0}.champ.xml", "NEAT_Controller");
		popFileSavePath = string.Format(Application.dataPath+"/Resources/{0}.pop.xml", "NEAT_Controller");
        superChampFileSavePath = string.Format(Application.dataPath + "/Resources/{0}.SUPERchamp.xml", "NEAT_Controller");
        normalFixedDeltaTime = Time.fixedDeltaTime;
        //print(champFileSavePath);
		browser = new FileBrowser (string.Format (UnityEngine.Application.dataPath + "/Resources/"), 1, new Rect(new Vector2(0,0), new Vector2(300,300)));
			}

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
    }

    public void StartEA()
    {        
        Utility.DebugLog = true;
        Utility.Log("Starting NEAT Training");
		print("Loading: " + popFileSavePath);
        _ea = experiment.CreateEvolutionAlgorithm(popFileSavePath);
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

    void ea_PauseEvent(object sender, EventArgs e)
    {
        Time.timeScale = 1;
        Utility.Log("Done ea'ing (and neat'ing)");

        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        // Save genomes to xml file.        
		DirectoryInfo dirInf = new DirectoryInfo(Application.dataPath+"/Resources");
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
        using (XmlWriter xw = XmlWriter.Create(popFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _ea.GenomeList);
        }
        // Also save the best genome

        using (XmlWriter xw = XmlWriter.Create(champFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
        }
        DateTime endTime = DateTime.Now;
        Utility.Log("Total time elapsed: " + (endTime - startTime));

        System.IO.StreamReader stream = new System.IO.StreamReader(popFileSavePath);

        EARunning = false;        
        
    }

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
			DirectoryInfo dirInf = new DirectoryInfo (Application.dataPath + "/Resources");
			if (!dirInf.Exists) {
				Debug.Log ("Creating subdirectory");
				dirInf.Create ();
			}
			using (XmlWriter xw = XmlWriter.Create (superChampFileSavePath, _xwSettings)) {
				experiment.SavePopulation (xw, new NeatGenome[] { _ea.CurrentChampGenome });
			}
		}
		((NEAT_GroupController)ControllerMap [box]).cleanUp ();
    }

    public void RunBest()
    {
		Time.timeScale = runBestSpeed;
		//Time.fixedDeltaTime = normalFixedDeltaTime * (1/runBestSpeed);
        NeatGenome genome = null;


        try
        {
			if (Config.NEAT)
			{
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

        // Decode the genome into a phenome (neural network).
        var phenome = genomeDecoder.Decode(genome);

		//print("Running best genome with fitness: "+ genome.BirthGeneration);

		GameObject groupController = Instantiate(Unit, Vector3.zero, Quaternion.identity) as GameObject;
		UnitController controller = groupController.GetComponent<UnitController>();
		((NEAT_GroupController)controller).setPathManager (simController.getCurrentTrack ().GetComponentInChildren<PathManager> ());

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
        if (GUI.Button(new Rect(10, 10, 100, 40), "Start EA"))
        {
            StartEA();
        }
        if (GUI.Button(new Rect(10, 60, 100, 40), "Stop EA"))
        {
            StopEA();
        }
        if (GUI.Button(new Rect(10, 110, 100, 40), "Open File"))
        {
			showFileBrowser ();
        }

		if(fileBrowserOpen)
		{
			if (browser.draw ())
			{
				if (browser.outputFile != null) {
					xmlPath = browser.outputFile.ToString ();
					RunBest ();
					browser.outputFile = null;
					fileBrowserOpen = false;
				}
				else
				{
					fileBrowserOpen = false;
				}
			}
		}



		GUI.Button(new Rect(10, Screen.height - 140, 150, 90), string.Format("Generation: {0}\nFitness: {1:0.00}\nSuperFitness: {3:0.00}\nTimeScale: {2}", Generation, Fitness, Time.timeScale, superFitness));
	}

	bool fileBrowserOpen = false;
	private void showFileBrowser()
	{
		fileBrowserOpen = true;
	}

}
