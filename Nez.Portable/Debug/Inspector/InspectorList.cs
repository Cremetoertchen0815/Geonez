﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.UI;

#if TRACE
namespace Nez
{
	/// <summary>
	///     container for a Component/PostProcessor/Transform and all of its inspectable properties
	/// </summary>
	public class InspectorList
    {
        private readonly List<Inspector> _inspectors;
        private CheckBox _enabledCheckbox;
        public string Name;
        public object Target;


        public InspectorList(object target)
        {
            Target = target;
            Name = target.GetType().Name;
            _inspectors = Inspector.GetInspectableProperties(target);
        }


        public InspectorList(Transform transform)
        {
            Name = "Transform";
            _inspectors = Inspector.GetTransformProperties(transform);
        }


        public void Initialize(Table table, Skin skin, float leftCellWidth)
        {
            table.GetRowDefaults().SetPadTop(10);
            table.Add(Name.Replace("PostProcessor", string.Empty)).GetElement<Label>().SetFontScale(1f)
                .SetFontColor(new Color(241, 156, 0));

            // if we have a component, stick a bool for enabled here
            if (Target != null)
            {
                _enabledCheckbox = new CheckBox(string.Empty, skin)
                {
                    ProgrammaticChangeEvents = false
                };

                if (Target is Component)
                    _enabledCheckbox.IsChecked = ((Component)Target).Enabled;
                else if (Target is PostProcessor)
                    _enabledCheckbox.IsChecked = ((PostProcessor)Target).Enabled;

                _enabledCheckbox.OnChanged += newValue =>
                {
                    if (Target is Component)
                        ((Component)Target).Enabled = newValue;
                    else if (Target is PostProcessor)
                        ((PostProcessor)Target).Enabled = newValue;
                };

                table.Add(_enabledCheckbox).Right();
            }

            table.Row();

            foreach (var i in _inspectors)
            {
                i.Initialize(table, skin, leftCellWidth);
                table.Row();
            }
        }


        public void Update()
        {
            foreach (var i in _inspectors)
                i.Update();
        }
    }
}
#endif