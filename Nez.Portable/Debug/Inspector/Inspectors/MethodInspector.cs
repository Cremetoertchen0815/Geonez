﻿using System;
using System.Reflection;
using Nez.UI;

#if TRACE
namespace Nez
{
    public class MethodInspector : Inspector
    {
        private Type _parameterType;

        // the TextField for our parameter if we have one
        private TextField _textField;


        public static bool AreParametersValid(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return true;

            if (parameters.Length > 1)
            {
                Debug.Warn(
                    $"method {parameters[0].Member.Name} has InspectorCallableAttribute but it has more than 1 parameter");
                return false;
            }

            var paramType = parameters[0].ParameterType;
            if (paramType == typeof(int) || paramType == typeof(float) || paramType == typeof(string) ||
                paramType == typeof(bool))
                return true;

            Debug.Warn(
                $"method {parameters[0].Member.Name} has InspectorCallableAttribute but it has an invalid paraemter type {paramType}");

            return false;
        }


        public override void Initialize(Table table, Skin skin, float leftCellWidth)
        {
            var button = new TextButton(_name, skin);
            button.OnClicked += OnButtonClicked;

            // we could have zero or 1 param
            var parameters = (_memberInfo as MethodInfo).GetParameters();
            if (parameters.Length == 0)
            {
                table.Add(button);
                return;
            }

            var parameter = parameters[0];
            _parameterType = parameter.ParameterType;

            _textField =
                new TextField(
                    _parameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(_parameterType).ToString() : "",
                    skin)
                {
                    ShouldIgnoreTextUpdatesWhileFocused = false
                };

            // add a filter for float/int
            if (_parameterType == typeof(float))
                _textField.SetTextFieldFilter(new FloatFilter());
            if (_parameterType == typeof(int))
                _textField.SetTextFieldFilter(new DigitsOnlyFilter());
            if (_parameterType == typeof(bool))
                _textField.SetTextFieldFilter(new BoolFilter());

            table.Add(button);
            table.Add(_textField).SetMaxWidth(70);
        }


        public override void Update()
        {
        }

        private void OnButtonClicked(Button button)
        {
            if (_parameterType == null)
            {
                (_memberInfo as MethodInfo).Invoke(_target, new object[] { });
            }
            else
            {
                // extract the param and properly cast it
                var parameters = new object[1];

                try
                {
                    if (_parameterType == typeof(float))
                        parameters[0] = float.Parse(_textField.GetText());
                    else if (_parameterType == typeof(int))
                        parameters[0] = int.Parse(_textField.GetText());
                    else if (_parameterType == typeof(bool))
                        parameters[0] = bool.Parse(_textField.GetText());
                    else
                        parameters[0] = _textField.GetText();

                    (_memberInfo as MethodInfo).Invoke(_target, parameters);
                }
                catch (Exception e)
                {
                    Debug.Error(e.ToString());
                }
            }
        }
    }
}
#endif