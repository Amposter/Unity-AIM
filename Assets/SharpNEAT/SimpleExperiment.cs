using UnityEngine;
using System.Collections;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.Decoders;
using System.Collections.Generic;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNEAT.Core;
using System;
using SharpNeat.Decoders.HyperNeat;
using SharpNeat.Network;

public class SimpleExperiment : INeatExperiment
{

    NeatEvolutionAlgorithmParameters _eaParams;
    NeatGenomeParameters _neatGenomeParams;
    string _name;
    int _populationSize;
    int _specieCount;
    NetworkActivationScheme _activationScheme;
    string _complexityRegulationStr;
    int? _complexityThreshold;
    string _description;
    Optimizer _optimizer;
    int _inputCount;
    int _outputCount;

    public string Name
    {
        get { return _name; }
    }

    public string Description
    {
        get { return _description; }
    }

    public int InputCount
    {
        get { return _inputCount; }
    }

    public int OutputCount
    {
        get { return _outputCount; }
    }

    public int DefaultPopulationSize
    {
        get { return _populationSize; }
    }

    public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
    {
        get { return _eaParams; }
    }

    public NeatGenomeParameters NeatGenomeParameters
    {
        get { return _neatGenomeParams; }
    }

    public void SetOptimizer(Optimizer se)
    {
        this._optimizer = se;
    }


    public void Initialize(string name, XmlElement xmlConfig)
    {
        Initialize(name, xmlConfig, 6, 3);
    }

    public void Initialize(string name, XmlElement xmlConfig, int input, int output)
    {
        _name = name;
        _populationSize = XmlUtils.GetValueAsInt(xmlConfig, "PopulationSize");
        _specieCount = XmlUtils.GetValueAsInt(xmlConfig, "SpecieCount");
        _activationScheme = ExperimentUtils.CreateActivationScheme(xmlConfig, "Activation");
        _complexityRegulationStr = XmlUtils.TryGetValueAsString(xmlConfig, "ComplexityRegulationStrategy");
        _complexityThreshold = XmlUtils.TryGetValueAsInt(xmlConfig, "ComplexityThreshold");
        _description = XmlUtils.TryGetValueAsString(xmlConfig, "Description");

        _eaParams = new NeatEvolutionAlgorithmParameters();
        _eaParams.SpecieCount = _specieCount;
        _neatGenomeParams = new NeatGenomeParameters();
        _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;

        _inputCount = input;
        _outputCount = output;
    }

    public List<NeatGenome> LoadPopulation(XmlReader xr)
    {
		if (Config.NEAT) {
			NeatGenomeFactory genomeFactory = (NeatGenomeFactory)CreateGenomeFactory ();
			return NeatGenomeXmlIO.ReadCompleteGenomeList (xr, false, genomeFactory);
		}
		else 
		{
			CppnGenomeFactory genomeFactory = (CppnGenomeFactory)CreateGenomeFactory (); 			
			return NeatGenomeXmlIO.ReadCompleteGenomeList (xr, true, genomeFactory);
		}
    }

    public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
    {
		if (Config.NEAT)
        	NeatGenomeXmlIO.WriteComplete(xw, genomeList, false);
		else
			NeatGenomeXmlIO.WriteComplete(xw, genomeList, true);
    }

    public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
    {
		{
			if (Config.NEAT) {
				return new NeatGenomeDecoder (_activationScheme);
			} else if (Config.HyperNEAT) {
				if (Config.substrateSetup == Config.HyperNEATSubstrateSetup.StopGo) {
					//These are the actual input/output nodes for the ANN. The config file's input/output nodes differ as they are for the CPPN, not the ANN.
					SubstrateNodeSet input = new SubstrateNodeSet (10);
					SubstrateNodeSet hidden = new SubstrateNodeSet (5);
					SubstrateNodeSet output = new SubstrateNodeSet (1);
					//   SubstrateNodeSet hidden = new SubstrateNodeSet(10);

					uint inputID = 1;
					uint hidID = 11;
					uint outputID = 16;

					if (!Config.substrateHiddenNodes) {
						outputID = 11;
					}

					//Left to right sensors
					for (int x = -4; x < 5; ++x, ++inputID)
						input.NodeList.Add (new SubstrateNode (inputID, new double[] { x, -1 }));

					//Back sensor
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, -2 }));

					//Hidden nodes
					if (Config.substrateHiddenNodes) {
						for (int x = -2; x < 2; ++x, ++hidID)
							hidden.NodeList.Add (new SubstrateNode (hidID, new double[] { x, 2 }));
					}
					//Output node
					output.NodeList.Add (new SubstrateNode (outputID, new double[] { 0, 4 }));

					List<SubstrateNodeSet> nodeSet;
					if (Config.substrateHiddenNodes) {
						nodeSet = new List<SubstrateNodeSet> (3);
						nodeSet.Add (input);
						nodeSet.Add (hidden);
						nodeSet.Add (output);
					} else {
						nodeSet = new List<SubstrateNodeSet> (2);
						nodeSet.Add (input);
						nodeSet.Add (output);
					}

					List<NodeSetMapping> nodeSetMapping;
					//Mapping
					if (Config.substrateHiddenNodes) {
						nodeSetMapping = new List<NodeSetMapping> (2);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
						nodeSetMapping.Add (NodeSetMapping.Create (1, 2, (double?)null));
					} else {
						nodeSetMapping = new List<NodeSetMapping> (1);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
					}
					//Substrate using steepend sigmoids, < 0.2 will not gen a weight 														
					Substrate substrate = new Substrate (nodeSet, DefaultActivationFunctionLibrary.CreateLibraryCppn (), 3, 0.2, 5, nodeSetMapping);
					//3 = Sine fn
					//Final decoder
					IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = //Cppn
						new HyperNeatDecoder (substrate, _activationScheme, _activationScheme, false);

					return genomeDecoder;
				} else if (Config.substrateSetup == Config.HyperNEATSubstrateSetup.SimpleDeviatePath) {
					//These are the actual input/output nodes for the ANN. The config file's input/output nodes differ as they are for the CPPN, not the ANN.
					SubstrateNodeSet input = new SubstrateNodeSet (12);
					SubstrateNodeSet hidden = new SubstrateNodeSet (5);
					SubstrateNodeSet output = new SubstrateNodeSet (2);
					//   SubstrateNodeSet hidden = new SubstrateNodeSet(10);

					uint inputID = 1;
					uint hidID = 13;
					uint outputID = 18;

					if (!Config.substrateHiddenNodes) {
						outputID = 13;
					}

					//Left to right sensors
					for (int x = -4; x < 5; ++x, ++inputID)
						input.NodeList.Add (new SubstrateNode (inputID, new double[] { x, -1 }));

					//Back sensor
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, -2 }));

					//Angle and distance
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, 0 }));
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, 1 }));

					//Hidden nodes
					if (Config.substrateHiddenNodes) {
						for (int x = -2; x < 2; ++x, ++hidID)
							hidden.NodeList.Add (new SubstrateNode (hidID, new double[] { x, 2 }));
					}

					//Output nodes: angle and speed
					output.NodeList.Add (new SubstrateNode (outputID++, new double[] { 0, 3 }));
					output.NodeList.Add (new SubstrateNode (outputID, new double[] { 0, 4 }));

					List<SubstrateNodeSet> nodeSet;
					if (Config.substrateHiddenNodes) {
						nodeSet = new List<SubstrateNodeSet> (3);
						nodeSet.Add (input);
						nodeSet.Add (hidden);
						nodeSet.Add (output);
					} else {
						nodeSet = new List<SubstrateNodeSet> (2);
						nodeSet.Add (input);
						nodeSet.Add (output);
					}

					List<NodeSetMapping> nodeSetMapping;
					//Mapping
					if (Config.substrateHiddenNodes) {
						nodeSetMapping = new List<NodeSetMapping> (2);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
						nodeSetMapping.Add (NodeSetMapping.Create (1, 2, (double?)null));
					} else {
						nodeSetMapping = new List<NodeSetMapping> (1);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
					}
					//Substrate using steepend sigmoids, < 0.2 will not gen a weight 														
					Substrate substrate = new Substrate (nodeSet, DefaultActivationFunctionLibrary.CreateLibraryCppn (), 3, 0.2, 5, nodeSetMapping);
					//3 = Sine fn
					//Final decoder
					IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = //Cppn
						new HyperNeatDecoder (substrate, _activationScheme, _activationScheme, false);

					return genomeDecoder;
				} else if (Config.substrateSetup == Config.HyperNEATSubstrateSetup.SingleObstacleInput) {
					//These are the actual input/output nodes for the ANN. The config file's input/output nodes differ as they are for the CPPN, not the ANN.
					SubstrateNodeSet input = new SubstrateNodeSet (2);
					SubstrateNodeSet hidden = new SubstrateNodeSet (5);
					SubstrateNodeSet output = new SubstrateNodeSet (2);
					//   SubstrateNodeSet hidden = new SubstrateNodeSet(10);

					uint inputID = 1;
					uint hidID = 3;
					uint outputID = 8;

					if (!Config.substrateHiddenNodes) {
						outputID = 8;
					}

					//Angle and distance to closest obstacle
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, 0 }));
					input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, 1 }));

					//Hidden nodes
					if (Config.substrateHiddenNodes) {
						for (int x = -2; x < 2; ++x, ++hidID)
							hidden.NodeList.Add (new SubstrateNode (hidID, new double[] { x, 2 }));
					}

					//Output nodes: angle and speed
					output.NodeList.Add (new SubstrateNode (outputID, new double[] { 0, 3 }));
					output.NodeList.Add (new SubstrateNode (outputID, new double[] { 0, 4 }));

					List<SubstrateNodeSet> nodeSet;
					if (Config.substrateHiddenNodes) {
						nodeSet = new List<SubstrateNodeSet> (3);
						nodeSet.Add (input);
						nodeSet.Add (hidden);
						nodeSet.Add (output);
					} else {
						nodeSet = new List<SubstrateNodeSet> (2);
						nodeSet.Add (input);
						nodeSet.Add (output);
					}

					List<NodeSetMapping> nodeSetMapping;
					//Mapping
					if (Config.substrateHiddenNodes) {
						nodeSetMapping = new List<NodeSetMapping> (2);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
						nodeSetMapping.Add (NodeSetMapping.Create (1, 2, (double?)null));
					} else {
						nodeSetMapping = new List<NodeSetMapping> (1);
						nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
					}
					//Substrate using steepend sigmoids, < 0.2 will not gen a weight 														
					Substrate substrate = new Substrate (nodeSet, DefaultActivationFunctionLibrary.CreateLibraryCppn (), 3, 0.2, 5, nodeSetMapping);
					//3 = Sine fn
					//Final decoder
					IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = //Cppn
						new HyperNeatDecoder (substrate, _activationScheme, _activationScheme, false);

					return genomeDecoder;
				} else if (Config.substrateSetup == Config.HyperNEATSubstrateSetup.ComplexStopGo) {
					//These are the actual input/output nodes for the ANN. The config file's input/output nodes differ as they are for the CPPN, not the ANN.

					SubstrateNodeSet input = new SubstrateNodeSet (10);
					SubstrateNodeSet hidden = new SubstrateNodeSet (5);
					SubstrateNodeSet output = new SubstrateNodeSet (1);
					//   SubstrateNodeSet hidden = new SubstrateNodeSet(10);

					uint inputID = 1;
					uint hidID = 11;
					uint outputID = 16;

					if (!Config.substrateHiddenNodes) {
						outputID = 11;


						//Sensors
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { -10, 0 })); // left
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { -9.24, 3.83 })); // left + 22.5*
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { -7.07, 7.07 })); // 45* left
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { -3.83, 9.24 })); // left + 62.5*
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 0, 10 })); // forward
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 3.83, 9.24 })); // forward + 22.5*
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 7.07, 7.07 })); // 45* right
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 9.24, 3.83 })); // forward + 62.5*
						input.NodeList.Add (new SubstrateNode (inputID++, new double[] { 10, 0 })); // right
						input.NodeList.Add (new SubstrateNode (inputID, new double[] { 0, -10 })); // back


						//Hidden nodes
						if (Config.substrateHiddenNodes) {
							for (int x = -2; x < 2; ++x, ++hidID)
								hidden.NodeList.Add (new SubstrateNode (hidID, new double[] { x, 3 }));
						}

						//Output node
						output.NodeList.Add (new SubstrateNode (outputID, new double[] { 0, 0 }));

						List<SubstrateNodeSet> nodeSet;
						if (Config.substrateHiddenNodes) {
							nodeSet = new List<SubstrateNodeSet> (3);
							nodeSet.Add (input);
							nodeSet.Add (hidden);
							nodeSet.Add (output);
						} else {
							nodeSet = new List<SubstrateNodeSet> (2);
							nodeSet.Add (input);
							nodeSet.Add (output);
						}

						List<NodeSetMapping> nodeSetMapping;
						//Mapping
						if (Config.substrateHiddenNodes) {
							nodeSetMapping = new List<NodeSetMapping> (2);
							nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
							nodeSetMapping.Add (NodeSetMapping.Create (1, 2, (double?)null));
						} else {
							nodeSetMapping = new List<NodeSetMapping> (1);
							nodeSetMapping.Add (NodeSetMapping.Create (0, 1, (double?)null));
						}
						//Substrate using steepend sigmoids, < 0.2 will not gen a weight 														
						Substrate substrate = new Substrate (nodeSet, DefaultActivationFunctionLibrary.CreateLibraryCppn (), 3, 0.2, 5, nodeSetMapping);
						//3 = Sine fn
						//Final decoder
						IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = //Cppn
							new HyperNeatDecoder (substrate, _activationScheme, _activationScheme, false);

						return genomeDecoder;
					}
				} else {
					throw new FormatException ("Either NEAT or HyperNEAT needs to be checked in the Config.cs file with the correct parameters!");
				}
				return default(IGenomeDecoder<NeatGenome, IBlackBox>);
			}
		}
    }


    public IGenomeFactory<NeatGenome> CreateGenomeFactory()
    {
		if (Config.NEAT)
        	return new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
		else
			return new CppnGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
    }

    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(string fileName)
    {
        List<NeatGenome> genomeList = null;
        IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();
        try
        {
            if (fileName.Contains("/.pop.xml"))
            {
                throw new Exception();
            }
            using (XmlReader xr = XmlReader.Create(fileName))
            {
                genomeList = LoadPopulation(xr);
            }
        }
        catch (Exception e1)
        {
            Utility.Log(fileName + " Error loading genome from file!\nLoading aborted.\n"
                                      + e1.Message + "\nJoe: " + fileName);

            genomeList = genomeFactory.CreateGenomeList(_populationSize, 0);

        }



        return CreateEvolutionAlgorithm(genomeFactory, genomeList);
    }

    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
    {
        return CreateEvolutionAlgorithm(_populationSize);
    }

    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
    {
        IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();

        List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

        return CreateEvolutionAlgorithm(genomeFactory, genomeList);
    }

    public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
    {
        IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
        ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);

        IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

        NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

        // Create black box evaluator       
        SimpleEvaluator evaluator = new SimpleEvaluator(_optimizer);

        IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();


		IGenomeListEvaluator<NeatGenome> innerEvaluator = new UnityListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _optimizer);

        //IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(innerEvaluator,
            //SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());

        //ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);
        ea.Initialize(innerEvaluator, genomeFactory, genomeList);

        return ea;
    }
}