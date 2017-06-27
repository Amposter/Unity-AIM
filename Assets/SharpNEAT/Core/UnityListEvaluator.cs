using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using System.Collections;
using UnityEngine;

namespace SharpNEAT.Core
{
    class UnityListEvaluator<TGenome, TPhenome> : IGenomeListEvaluator<TGenome>
        where TGenome : class, IGenome<TGenome>
        where TPhenome : class
	{
        
        readonly IGenomeDecoder<TGenome, TPhenome> _genomeDecoder;
        readonly IPhenomeEvaluator<TPhenome> _phenomeEvaluator;
		Optimizer _optimizer;

        #region Constructor

        /// <summary>
        /// Construct with the provided IGenomeDecoder and IPhenomeEvaluator.
        /// </summary>
        public UnityListEvaluator(IGenomeDecoder<TGenome, TPhenome> genomeDecoder,
                                         IPhenomeEvaluator<TPhenome> phenomeEvaluator,
											Optimizer optimizer)
        {
            _genomeDecoder = genomeDecoder;
            _phenomeEvaluator = phenomeEvaluator;
			_optimizer = optimizer;
        }

        #endregion

        public ulong EvaluationCount
        {
            get { return _phenomeEvaluator.EvaluationCount; }
        }

        public bool StopConditionSatisfied
        {
            get { return _phenomeEvaluator.StopConditionSatisfied; }
        }

        public IEnumerator Evaluate(IList<TGenome> genomeList)
        {
            yield return Coroutiner.StartCoroutine(evaluateList(genomeList));
        }

        private IEnumerator evaluateList(IList<TGenome> genomeList)
        {
			Dictionary<TGenome, TPhenome> dict = new Dictionary<TGenome, TPhenome>();
			Dictionary<TGenome, FitnessInfo[]> fitnessDict = new Dictionary<TGenome, FitnessInfo[]>();

			_phenomeEvaluator.Reset();

			float genomeNum = 0;

            foreach (TGenome genome in genomeList)
            {
				genomeNum++;
				if (((genomeNum / genomeList.Count) * 100) % 20 == 0)
				{
					Debug.Log ("Generation "+((genomeNum / genomeList.Count) * 100)+"% evaluated...");
				}

				TPhenome phenome = _genomeDecoder.Decode (genome);

				if (null == phenome) {   // Non-viable genome.
					genome.EvaluationInfo.SetFitness (0.0);
					genome.EvaluationInfo.AuxFitnessArr = null;
					continue;
				}

				dict.Add(genome, phenome);

				for (int i = 0; i < _optimizer.Trials; i++)
				{

					if (!fitnessDict.Keys.Contains(genome))
					{
						fitnessDict.Add(genome, new FitnessInfo[_optimizer.Trials]);
					}

					//_phenomeEvaluator.Reset();

					yield return Coroutiner.StartCoroutine(_phenomeEvaluator.Evaluate(phenome));

					//yield return new WaitForSeconds(_optimizer.TrialDuration);
				
					FitnessInfo fitnessInfo = _phenomeEvaluator.GetLastFitness(phenome);

					fitnessDict[genome][i] = fitnessInfo;

					//Debug.Log ("Trial "+(i + 1)+" fitness: "+fitnessInfo._fitness);
				}

				double fitness = 0;

				for (int i = 0; i < _optimizer.Trials; i++)
				{

					fitness += fitnessDict[genome][i]._fitness;

				}
				var fit = fitness;
				fitness /= _optimizer.Trials; // Averaged fitness

				genome.EvaluationInfo.SetFitness(fitness);
				genome.EvaluationInfo.AuxFitnessArr = fitnessDict[genome][0]._auxFitnessArr;
				//Debug.Log ("Final fitness for genome "+genomeNum+": "+fitness);
            }
        }

        public void Reset()
        {
            _phenomeEvaluator.Reset();
        }
    }
}
