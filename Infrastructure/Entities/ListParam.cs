//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;

namespace ForexStrategyBuilder.Infrastructure.Entities
{
    public class ListParam
    {
        public ListParam()
        {
            Caption = String.Empty;
            ItemList = new[] {""};
            Index = 0;
            Text = String.Empty;
            Enabled = false;
            ToolTip = String.Empty;
        }

        /// <summary>
        ///     Gets or sets the text describing the parameter.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        ///     Gets or sets the list of parameter values.
        /// </summary>
        public string[] ItemList { get; set; }

        /// <summary>
        ///     Gets or sets the text associated whit this parameter.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the index specifying the currently selected item.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the control can respond to user interaction.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the text of tool tip associated with this control.
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        ///     Returns a copy
        /// </summary>
        public ListParam Clone()
        {
            var listParam = new ListParam
                {
                    Caption = Caption,
                    ItemList = new string[ItemList.Length],
                    Index = Index,
                    Text = Text,
                    Enabled = Enabled,
                    ToolTip = ToolTip
                };
            ItemList.CopyTo(listParam.ItemList, 0);
            return listParam;
        }
    }
}