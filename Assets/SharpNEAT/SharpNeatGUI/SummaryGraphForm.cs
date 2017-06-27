﻿/* ***************************************************************************
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
using System;
using System.Windows.Forms;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Utility;
using ZedGraph;
using System.Drawing;

namespace SharpNeatGUI
{
    /// <summary>
    /// Form for displaying a graph plot of summary information (e.g. distribution curves).
    /// </summary>
    public partial class SummaryGraphForm : Form
    {
        AbstractGenerationalAlgorithm<NeatGenome> _ea;
        SummaryDataSource[] _dataSourceArray;
        PointPairList[] _pointPlotArray;
        GraphPane _graphPane;
        Color[] _plotColorArr = new Color[] { Color.LightSlateGray, Color.LightBlue, Color.LightGreen };

        #region Constructor

        /// <summary>
        /// Construct the form with the provided details and data sources.
        /// </summary>
        public SummaryGraphForm(string title, string xAxisTitle, string y1AxisTitle, string y2AxisTitle,
                         SummaryDataSource[] dataSourceArray, AbstractGenerationalAlgorithm<NeatGenome> ea)
        {
            //InitializeComponent();

            this.Text = string.Format("SharpNEAT - {0}", title);
            _dataSourceArray = dataSourceArray;
            InitGraph(title, xAxisTitle, y1AxisTitle, y2AxisTitle, dataSourceArray);

            _ea = ea;
            if(null != ea) {
                _ea.UpdateEvent += new EventHandler(_ea_UpdateEvent);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when a new evolution algorithm is initialized. Clean up any existing event listeners and
        /// connect up to the new evolution algorithm.
        /// </summary>
        public void Reconnect(AbstractGenerationalAlgorithm<NeatGenome> ea)
        {
            // Clean up.
            if(null != _ea) {
                _ea.UpdateEvent -= new EventHandler(_ea_UpdateEvent);
            }

            foreach(PointPairList ppl in _pointPlotArray) {
                ppl.Clear();
            }

            // Reconnect.
            _ea = ea;
            _ea.UpdateEvent += new EventHandler(_ea_UpdateEvent);
        }

        #endregion

        #region Private Methods

        private void InitGraph(string title, string xAxisTitle, string y1AxisTitle, string y2AxisTitle, SummaryDataSource[] dataSourceArray)
        {
           

            // Create point-pair lists and bind them to the graph control.
            int sourceCount = dataSourceArray.Length;
            _pointPlotArray = new PointPairList[sourceCount];
            for(int i=0; i<sourceCount; i++)
            {
                SummaryDataSource ds = dataSourceArray[i];
                _pointPlotArray[i] =new PointPairList();

                Color color = _plotColorArr[i % 3];
                BarItem barItem = _graphPane.AddBar(ds.Name, _pointPlotArray[i], color);
                barItem.Bar.Fill = new Fill(color);
              //  _graphPane.BarSettings.MinClusterGap = 0;

                barItem.IsY2Axis = (ds.YAxis == 1);
            }
        }

        private void EnsurePointPairListLength(PointPairList ppl, int length)
        {
            int delta = length-ppl.Count;
            
            if(delta > 0)
            {   // Add additional points.
                for(int i=0; i<delta; i++) {
                    ppl.Add(0.0, 0.0);
                }
            }
            else if(delta < 0)
            {   // Remove excess points.
              //  ppl.RemoveRange(length, -delta);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle update event from the evolution algorithm - update the view.
        /// </summary>
        public void _ea_UpdateEvent(object sender, EventArgs e)
        {
            // Switch execution to GUI thread if necessary.
            if(this.InvokeRequired)
            {
                // Must use Invoke(). BeginInvoke() will execute asynchronously and the evolution algorithm therefore 
                // may have moved on and will be in an intermediate and indeterminate (between generations) state.
                this.Invoke(new MethodInvoker(delegate() 
                {
                    if(this.IsDisposed) {
                        return;
                    }

                    // Update plot points for each series in turn.
                    int sourceCount = _dataSourceArray.Length;
                    for(int i=0; i<sourceCount; i++)
                    {
                        SummaryDataSource ds = _dataSourceArray[i];
                        Point2DDouble[] pointArr = ds.GetPointArray();
                        PointPairList ppl = _pointPlotArray[i];
                        EnsurePointPairListLength(ppl, pointArr.Length);

                        for(int j=0; j<pointArr.Length; j++)
                        {
                       //     ppl[j].X = pointArr[j].X;
                         //   ppl[j].Y = pointArr[j].Y;
                        }
                    }

                    // Trigger graph to redraw.
                   // zed.AxisChange();
                    Refresh();
                }));
            }
        }

        private void GenomeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(null != _ea) {
                _ea.UpdateEvent -= new EventHandler(_ea_UpdateEvent);
            }
        }

        #endregion
    }
}
