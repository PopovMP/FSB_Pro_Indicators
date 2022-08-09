//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using static System.String;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    /// <summary>
    ///     Describes a parameter who can be used or not.
    /// </summary>
    public class CheckParam
    {
        /// <summary>
        ///     The default constructor.
        /// </summary>
        public CheckParam()
        {
            Caption = Empty;
            Enabled = false;
            Checked = false;
            ToolTip = Empty;
        }

        /// <summary>
        ///     Gets or sets the text describing the parameter.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the control is checked.
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the control can respond to user interaction.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the text of tool tip associated with this control.
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        ///     Returns a copy of the class.
        /// </summary>
        public CheckParam Clone()
        {
            return new CheckParam
                {
                    Caption = Caption,
                    Enabled = Enabled,
                    Checked = Checked,
                    ToolTip = ToolTip
                };
        }
    }
}