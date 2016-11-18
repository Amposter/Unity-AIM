/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using SharpNeat.View.Graph;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;

namespace SharpNeat.Domains
{
    /// <summary>
    /// GUI control for visualizing CPPN genomes.
    /// </summary>
    public partial class CppnGenomeView : AbstractGenomeView
    {
        NetworkGraphFactory _graphFactory = new NetworkGraphFactory();
        IOGraphViewportPainter _viewportPainter;
		INetworkDefinition _networkDefinition;

        #region Constructor

        /// <summary>
        /// Construct with the provided CPPN activation function library to draw CPPNs with (genome nodes contain an index into this library).
        /// </summary>
        /// <param name="actFnLib"></param>
        public CppnGenomeView(IActivationFunctionLibrary actFnLib)
        {
            InitializeComponent();
            graphControl1.ViewportPainter = _viewportPainter = new IOGraphViewportPainter(new CppnGraphPainter(actFnLib));
        }

		public CppnGenomeView(IActivationFunctionLibrary actFnLib, INetworkDefinition definition)
		{
			InitializeComponent();
			graphControl1.ViewportPainter = _viewportPainter = new IOGraphViewportPainter(new CppnGraphPainter(actFnLib));
			_networkDefinition = definition;
		}


        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh/update the view with the provided genome.
        /// </summary>
        public override void RefreshView(object genome)
        {
            NeatGenome neatGenome = genome as NeatGenome;
            if(null == neatGenome) {
                return;
            }

			IOGraph graph;
			if (Config.substrateVis)
				graph = _graphFactory.CreateGraph(neatGenome);
			else
				graph = _graphFactory.CreateGraph(_networkDefinition);
            _viewportPainter.IOGraph = graph;
            graphControl1.RefreshImage();
        }

        #endregion
    }
}
