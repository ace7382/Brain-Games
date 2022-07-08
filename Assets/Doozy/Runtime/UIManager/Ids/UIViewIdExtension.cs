// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

//.........................
//.....Generated Class.....
//.........................
//.......Do not edit.......
//.........................

using System.Collections.Generic;
// ReSharper disable All
namespace Doozy.Runtime.UIManager.Containers
{
    public partial class UIView
    {
        public static IEnumerable<UIView> GetViews(UIViewId.GameManagement id) => GetViews(nameof(UIViewId.GameManagement), id.ToString());
        public static void Show(UIViewId.GameManagement id, bool instant = false) => Show(nameof(UIViewId.GameManagement), id.ToString(), instant);
        public static void Hide(UIViewId.GameManagement id, bool instant = false) => Hide(nameof(UIViewId.GameManagement), id.ToString(), instant);

        public static IEnumerable<UIView> GetViews(UIViewId.Inventory id) => GetViews(nameof(UIViewId.Inventory), id.ToString());
        public static void Show(UIViewId.Inventory id, bool instant = false) => Show(nameof(UIViewId.Inventory), id.ToString(), instant);
        public static void Hide(UIViewId.Inventory id, bool instant = false) => Hide(nameof(UIViewId.Inventory), id.ToString(), instant);
    }
}

namespace Doozy.Runtime.UIManager
{
    public partial class UIViewId
    {
        public enum GameManagement
        {
            Countdown,
            EndLevel,
            GamePlayScreen,
            TutorialScreen
        }

        public enum Inventory
        {
            PartyManagement
        }    
    }
}