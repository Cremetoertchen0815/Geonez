#region File Description

//-----------------------------------------------------------------------------
// Generate file menu layout.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.GeonBit.UI.Entities;
using Nez.GeonBit.UI.Exceptions;

namespace Nez.GeonBit.UI.Utils;

/// <summary>
///     A helper class to generate simple file-menu (top navbar) using panels and dropdown entities.
/// </summary>
public static class SimpleFileMenu
{
	/// <summary>
	///     Create the file menu and return the root panel.
	///     The result would be a panel containing a group of dropdown entities, which implement the file menu layout.
	///     The id of every dropdown is "menu-[menu-title]".
	///     Note: the returned file menu panel comes without parent, you need to add it to your UI tree manually.
	/// </summary>
	/// <param name="layout">Layout to create file menu for.</param>
	/// <param name="skin">Skin to use for panels and dropdown of this file menu.</param>
	/// <returns>Menu root panel.</returns>
	public static Panel Create(MenuLayout layout, PanelSkin skin = PanelSkin.Simple)
    {
        // create the root panel
        var rootPanel = new Panel(new Vector2(0, DropDown.SelectedPanelHeight), skin, Anchor.TopLeft)
        {
            Padding = Vector2.Zero
        };

        // create menus
        foreach (var menu in layout.Layout)
        {
            // create dropdown and all its items
            var dropdown = new DropDown(new Vector2(menu.Width, -1), Anchor.AutoInline, null, false);
            rootPanel.AddChild(dropdown);
            foreach (var item in menu.Items) dropdown.AddItem(item);
            dropdown.AutoSetListHeight = true;

            // set menu title and id
            dropdown.DefaultText = menu.Title;
            dropdown.Identifier = "menu-" + menu.Title;

            // disable dropdown selection (will only trigger event and unselect)
            dropdown.DontKeepSelection = true;

            // set callbacks
            for (var i = 0; i < menu.Items.Count; ++i)
            {
                var callback = menu.Actions[i];
                if (callback != null) dropdown.OnSelectedSpecificItem(menu.Items[i], callback);
            }
        }

        // return the root panel
        return rootPanel;
    }

	/// <summary>
	///     Class used to define the file menu layout.
	/// </summary>
	public class MenuLayout
    {
	    /// <summary>
	    ///     Menus layout.
	    /// </summary>
	    internal List<Menu> Layout { get; } = new();

	    /// <summary>
	    ///     Add a top menu.
	    /// </summary>
	    /// <param name="title">Menu title.</param>
	    /// <param name="width">Menu width.</param>
	    public void AddMenu(string title, float width)
        {
            var newMenu = new Menu
            {
                Title = title,
                Width = width
            };
            Layout.Add(newMenu);
        }

	    /// <summary>
	    ///     Adds an item to a menu.
	    /// </summary>
	    /// <param name="menuTitle">Menu title to add item to.</param>
	    /// <param name="item">Item text.</param>
	    /// <param name="onClick">On-click action.</param>
	    public void AddItemToMenu(string menuTitle, string item, Action onClick)
        {
            foreach (var menu in Layout)
                if (menu.Title == menuTitle)
                {
                    menu.Items.Add(item);
                    menu.Actions.Add(onClick);
                    return;
                }

            throw new NotFoundException("Menu with title '" + menuTitle + "' is not defined!");
        }

	    /// <summary>
	    ///     A single menu in the file menu navbar.
	    /// </summary>
	    internal class Menu
        {
	        /// <summary>
	        ///     Actions attached to menu.
	        /// </summary>
	        public List<Action> Actions = new();

	        /// <summary>
	        ///     Items under this menu.
	        /// </summary>
	        public List<string> Items = new();

	        /// <summary>
	        ///     Menu title.
	        /// </summary>
	        public string Title;

	        /// <summary>
	        ///     Menu width.
	        /// </summary>
	        public float Width;
        }
    }
}