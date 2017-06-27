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

using System;
using System.Windows.Forms;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

namespace SharpNeatGUI
{
    /// <summary>
    /// Form for genome visualization. A generic form that supports all genome types by wrapping an AbstractGenomeView
    /// (the control does the actual visual rendering).
    /// </summary>
    public partial class GenomeForm : Form
    {
        AbstractGenomeView _genomeViewControl;
		NeatGenome _genome;

        #region Constructor

        /// <summary>
        /// Construct with the provided form title, genome view/renderer and evolution algorithm. We listen to update events
        /// from the evolution algorithm and cleanly detach from it when this form closes.
        /// </summary>
        public GenomeForm(string title, AbstractGenomeView genomeViewControl, NeatGenome genome)
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenomeForm));
			this.SuspendLayout();
			// 
			// GenomeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(340, 317);
			//this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GenomeForm";
			this.Text = "GenericForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenomeForm_FormClosing);
			this.ResumeLayout(false);

            this.Text = title;

            _genomeViewControl = genomeViewControl;
            genomeViewControl.Dock = DockStyle.Fill;
            this.Controls.Add(genomeViewControl);

			_genome = genome;

			this.Visible = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when a new evolution algorithm is initialized. Clean up any existing event listeners and
        /// connect up to the new evolution algorithm.
        /// </summary>
        public void Reconnect(AbstractGenerationalAlgorithm<NeatGenome> ea)
        {
         
        }

        /// <summary>
        /// Refresh view.
        /// </summary>
        public void RefreshView()
        {
                _genomeViewControl.RefreshView(_genome);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle update event from the evolution algorithm - update the view.
        /// </summary>
        public void _ea_UpdateEvent(object sender, EventArgs e)
        {
           
        }

        private void GenomeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
         
        }

        #endregion
    }
}
