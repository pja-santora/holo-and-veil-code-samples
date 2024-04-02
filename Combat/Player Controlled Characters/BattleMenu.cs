using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : MonoBehaviour
{
    public MenuControllerBattleMenu action_menu_controller;
    public MenuControllerAdvanced attack_menu_controller;
    public MenuControllerAdvanced duet_menu_controller;
    public MenuControllerAdvanced gambit_menu_controller;
    public MenuControllerAdvanced item_menu_controller;

    public GameObject cycle_menu_object;
    public CycleMenu cycle_menu;

    public Animator action_menu_animator;

    public GameObject[] sub_menus;
    public VerticalScrollMenu[] sub_menu_scrolls;
    public Animator[] ActionMenuOptionAnimators;
    public Image[] ActionMenuBackgrounds;

    [HideInInspector] public List<BattleMenuOption> AttackMenuOptions;
    [HideInInspector] public List<DuoMenuOption> DuetMenuOptions;
    [HideInInspector] public List<BattleMenuOption> GambitMenuOptions;
    [HideInInspector] public List<ItemMenuOption> ItemMenuOptions;

    public enum State { action, attacks, duets, gambits, items, selector, cycle, off }
    [HideInInspector] public State _state;
    [HideInInspector] public State previous_state;

    public static string other_actor = "";

    private const int START_OFFSET_X = 500;
    private const int START_OFFSET_Y = -60;
    private const int SUB_MENU_OFFSET = -100;

    private int previous_action_menu_index;
    private int previous_sub_menu_index;
    private string _character;

    private DescriptionBox description_box;
    private SelectorSystem selector_system;
    private BattleHUD player_hud;
    private BattleCharacter attached_character;

    public void Set(BattleCharacter battle_character)
    {
        SetAttacks(battle_character);
        SetDuets(battle_character);
        SetGambits(battle_character);
        SetItems();

        _character = battle_character._name;
        SetCharacterColor(_character);

        selector_system = GameObject.Find("Selectors").GetComponent<SelectorSystem>();
        description_box = GameObject.Find("_description_box").GetComponent<DescriptionBox>();
        player_hud = GameObject.Find("player_HUD").GetComponent<BattleHUD>();
        attached_character = battle_character;

        Reset();
        previous_action_menu_index = -1;
        previous_sub_menu_index = -1;
        action_menu_controller.index = 0;
    }

    private void SetAttacks(BattleCharacter battle_character)
    {
        int i = 0;
        foreach (Attack attack in battle_character.Attacks)
        {
            // instantiate the menu item
            GameObject menu_option_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/0_Buttons/sub_battle_menu_item"),
                attack_menu_controller.gameObject.transform);
            menu_option_prefab.transform.localPosition = new Vector2(START_OFFSET_X, i * SUB_MENU_OFFSET + START_OFFSET_Y);

            BattleMenuOption option = menu_option_prefab.GetComponent<BattleMenuOption>();
            option.Set(attack._name, attack._cost, attack._side, attack._target);

            // attach the right menu controller
            option.GetComponent<MenuButtonAdvanced>().menu_controller = attack_menu_controller;
            option.GetComponent<MenuButtonAdvanced>().current_index = i;

            AttackMenuOptions.Add(option);
            i += 1;
        }

        // set the menu controller
        attack_menu_controller.max_index = AttackMenuOptions.Count - 1;

        sub_menu_scrolls[0].SetDelimiters(attack_menu_controller.max_index);
    }

    private void SetDuets(BattleCharacter battle_character)
    {
        foreach(string other_character in Player.Party)
        {
            if (!battle_character._name.Equals(other_character))
            {
                if (SupportSystem.ConversationsUnlocked[SupportSystem.Order(battle_character._name, other_character)][0] && 
                    !SupportSystem.ConversationsViewable[SupportSystem.Order(battle_character._name, other_character)][0])
                {
                    // instantiate the menu item in the right place
                    GameObject menu_option_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/0_Buttons/duet_battle_menu_item"),
                        duet_menu_controller.gameObject.transform);
                    menu_option_prefab.transform.localPosition = new Vector2(START_OFFSET_X, START_OFFSET_Y);

                    DuoMenuOption option = menu_option_prefab.GetComponent<DuoMenuOption>();
                    var duo_action = Duet.AllActions[SupportSystem.Order(battle_character._name, other_character) + " : C"];
                    option.Set(duo_action._name, duo_action._cost, duo_action._side, duo_action._target,
                        battle_character._name, other_character, 'C');

                    // attach the right menu controller
                    option.GetComponent<MenuButtonAdvanced>().menu_controller = duet_menu_controller;
                    option.GetComponent<MenuButtonAdvanced>().current_index = 0;

                    DuetMenuOptions.Add(option);
                }
            }
        }

        // set the menu controller
        duet_menu_controller.max_index = DuetMenuOptions.Count - 1;

        sub_menu_scrolls[1].SetDelimiters(duet_menu_controller.max_index);
    }

    private void SetGambits(BattleCharacter battle_character)
    {
        int i = 0;
        foreach (Gambit tactic in battle_character.Gambits)
        {
            // instantiate the menu item
            GameObject menu_option_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/0_Buttons/sub_battle_menu_item"),
                gambit_menu_controller.gameObject.transform);
            menu_option_prefab.transform.localPosition = new Vector2(START_OFFSET_X, i * SUB_MENU_OFFSET + START_OFFSET_Y);

            BattleMenuOption option = menu_option_prefab.GetComponent<BattleMenuOption>();
            option.Set(tactic._name, tactic._cost, tactic._side, tactic._target);

            // attach the right menu controller
            option.GetComponent<MenuButtonAdvanced>().menu_controller = gambit_menu_controller;
            option.GetComponent<MenuButtonAdvanced>().current_index = i;

            // finish
            GambitMenuOptions.Add(option);
            i += 1;
        }

        // set the menu controller
        gambit_menu_controller.max_index = GambitMenuOptions.Count - 1;

        sub_menu_scrolls[2].SetDelimiters(gambit_menu_controller.max_index);
    }

    private void SetItems()
    {
        int i = 0;
        foreach (UseableItem item in Player.ObtainedItems)
        {
            // instantiate the inventory menu item
            GameObject menu_option_prefab = (GameObject)Instantiate(Resources.Load("Battle/UI/0_Buttons/item_battle_menu_item"),
                item_menu_controller.gameObject.transform);
            menu_option_prefab.transform.localPosition = new Vector2(START_OFFSET_X, i * SUB_MENU_OFFSET + START_OFFSET_Y);

            ItemMenuOption option = menu_option_prefab.GetComponent<ItemMenuOption>();

            if (item._function == UseableItem.Function.battle)
            {
                option.Set(item._name, item._owned, item._function, item._side, item._target);
            }
            else
            {
                option.Set(item._name, item._owned, item._function);
            }

            // attach the right menu controller
            option.GetComponent<MenuButtonAdvanced>().menu_controller = item_menu_controller;
            option.GetComponent<MenuButtonAdvanced>().current_index = i;

            ItemMenuOptions.Add(option);
            i += 1;
        }

        // set the menu controller
        item_menu_controller.max_index = ItemMenuOptions.Count - 1;

        sub_menu_scrolls[3].SetDelimiters(item_menu_controller.max_index);
    }

    public void ReSetItems()
    {
        ClearItemMenu();
        SetItems();
    }

    private void ClearItemMenu()
    {
        for (int i = ItemMenuOptions.Count - 1; i >= 0; --i)
        {
            Destroy(ItemMenuOptions[i].gameObject);
            ItemMenuOptions.RemoveAt(i);
        }
        item_menu_controller.max_index = -1;
    }

    private void Update()
    {
        if (_state == State.action && action_menu_controller.tracker == MenuTracker.on)
        {
            CheckActionOptions();
            if (action_menu_controller.index != previous_action_menu_index)
            {
                previous_action_menu_index = action_menu_controller.index;
                SetActionMenuAnimator();
            }
            
            if (Command.Select())
            {
                if (ActionMenuOptionAnimators[action_menu_controller.index].GetBool("Greyed"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                }

                if (action_menu_controller.index == 0 && AttackMenuOptions.Count > 0)
                {
                    ActionMenuOptionAnimators[action_menu_controller.index].SetBool("pressed", true);
                    SwitchToAttacksMenu();
                }
                else if (action_menu_controller.index == 1 && DuetMenuOptions.Count > 0)
                {
                    ActionMenuOptionAnimators[action_menu_controller.index].SetBool("pressed", true);
                    SwitchToDuetsMenu();
                }
                else if (action_menu_controller.index == 2 && GambitMenuOptions.Count > 0)
                {
                    ActionMenuOptionAnimators[action_menu_controller.index].SetBool("pressed", true);
                    SwitchToGambitsMenu();
                }
                else if (action_menu_controller.index == 3 && ItemMenuOptions.Count > 0)
                {
                    ActionMenuOptionAnimators[action_menu_controller.index].SetBool("pressed", true);
                    SwitchToItemsMenu();
                }
            }
        }
        else if (_state == State.attacks && attack_menu_controller.tracker == MenuTracker.on)
        {
            if (attack_menu_controller.index != previous_sub_menu_index)
            {
                previous_sub_menu_index = attack_menu_controller.index;
                description_box.ShowDescriptionBox(AttackMenuOptions[attack_menu_controller.index]._name.text);
            }
            
            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                SwitchToActionsMenu();
            }
            else if (Command.Select() && AttackMenuOptions[attack_menu_controller.index].CanSelect)
            {
                if (AttackMenuOptions[attack_menu_controller.index]._animator.GetBool("Greyed"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                }

                SetToSelector(attached_character._index, AttackMenuOptions[attack_menu_controller.index]._side,
                    AttackMenuOptions[attack_menu_controller.index]._target, attack_menu_controller.index);
            }
        }
        else if (_state == State.duets && duet_menu_controller.tracker == MenuTracker.on)
        {
            if (duet_menu_controller.index != previous_sub_menu_index)
            {
                previous_sub_menu_index = duet_menu_controller.index;
                description_box.ShowDescriptionBox(DuetMenuOptions[duet_menu_controller.index]._name.text);
            }
            
            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                SwitchToActionsMenu();
            }
            else if (Command.Select() && DuetMenuOptions[duet_menu_controller.index].CanSelect)
            {
                if (DuetMenuOptions[duet_menu_controller.index]._animator.GetBool("Greyed"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                }

                SetToSelector(attached_character._index, DuetMenuOptions[duet_menu_controller.index]._side,
                    DuetMenuOptions[duet_menu_controller.index]._target, duet_menu_controller.index);
            }
        }
        else if (_state == State.gambits && gambit_menu_controller.tracker == MenuTracker.on)
        {
            if (gambit_menu_controller.index != previous_sub_menu_index)
            {
                previous_sub_menu_index = gambit_menu_controller.index;
                description_box.ShowDescriptionBox(GambitMenuOptions[gambit_menu_controller.index]._name.text);
            }
            
            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                SwitchToActionsMenu();
            }
            else if (Command.Select() && GambitMenuOptions[gambit_menu_controller.index].CanSelect)
            {
                if (GambitMenuOptions[gambit_menu_controller.index]._animator.GetBool("Greyed"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                }

                if (GambitMenuOptions[gambit_menu_controller.index]._name.text.Equals("Cycle"))
                {
                    SwitchToCycleMenu();
                }
                else
                {
                    SetToSelector(attached_character._index, GambitMenuOptions[gambit_menu_controller.index]._side,
                        GambitMenuOptions[gambit_menu_controller.index]._target, gambit_menu_controller.index);
                }
            }
        }
        else if (_state == State.items && item_menu_controller.tracker == MenuTracker.on)
        {
            if (item_menu_controller.index != previous_sub_menu_index)
            {
                previous_sub_menu_index = item_menu_controller.index;
                description_box.ShowItemDescriptionBox(ItemMenuOptions[item_menu_controller.index]._name.text);
            }

            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                SwitchToActionsMenu();
            }
            else if (Command.Select() && ItemMenuOptions[item_menu_controller.index].CanSelect)
            {
                if (ItemMenuOptions[item_menu_controller.index]._animator.GetBool("Greyed"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                }

                SetToSelector(attached_character._index, ItemMenuOptions[item_menu_controller.index]._side,
                    ItemMenuOptions[item_menu_controller.index]._target, item_menu_controller.index, 
                    ItemMenuOptions[item_menu_controller.index].item_name);
            }
        }
        else if (_state == State.selector)
        {


            if (previous_state == State.attacks)
            {
                description_box.ShowInstructionBox(AttackMenuOptions[attack_menu_controller.index]._name.text);
                ReflectActionCost(AttackMenuOptions[attack_menu_controller.index].action_cost);
            }
            else if (previous_state == State.duets)
            {
                description_box.ShowInstructionBox(DuetMenuOptions[duet_menu_controller.index]._name.text);
                ReflectActionCost(DuetMenuOptions[duet_menu_controller.index].action_cost);
            }
            else if (previous_state == State.gambits)
            {
                description_box.ShowInstructionBox(GambitMenuOptions[gambit_menu_controller.index]._name.text);
                ReflectActionCost(GambitMenuOptions[gambit_menu_controller.index].action_cost);
            }
            else if (previous_state == State.items)
            {
                description_box.HideBox();
            }

            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                if (previous_state == State.attacks)
                {
                    SwitchToAttacksMenu();
                }
                else if (previous_state == State.duets)
                {
                    SwitchToDuetsMenu();
                }
                else if (previous_state == State.gambits)
                {
                    SwitchToGambitsMenu();
                }
                else if (previous_state == State.items)
                {
                    SwitchToItemsMenu();
                }
                description_box.HideInstruction();
                SetStatToNormal();
            }
            else if (Command.Select())
            {
                if (previous_state == State.duets)
                {
                    other_actor = DuetMenuOptions[duet_menu_controller.index].second_character_name;
                }

                SoundEffectSystem.Play(SFX.menu_select);
                description_box.HideBox();
                description_box.HideNames();
                description_box.HideInstruction();
                SetStatToNormal();
                Reset();
            }
        }
        else if (_state == State.cycle)
        {
            if (Command.Back())
            {
                SoundEffectSystem.Play(SFX.menu_back);
                SwitchToGambitsMenu();
            }
            else if (Command.Select() && cycle_menu.CycleMenuOptions[cycle_menu.menu_controller.index].CanSelect)
            {
                if (cycle_menu.CycleMenuOptions[cycle_menu.menu_controller.index].name_value.Equals("Cancel"))
                {
                    SoundEffectSystem.Play(SFX.menu_back);
                    SwitchToGambitsMenu();
                }
                else
                {
                    SoundEffectSystem.Play(SFX.menu_select);
                    description_box.HideBox();
                    description_box.HideNames();
                    Reset();
                }
            }
        }
    }

    public void Reset()
    {
        enabled = false;
        _state = State.off;

        action_menu_animator.SetBool("OnAttacks", false);
        action_menu_animator.SetBool("OnDuets", false);
        action_menu_animator.SetBool("OnGambits", false);
        action_menu_animator.SetBool("OnItems", false);
        action_menu_animator.SetBool("OnMenu", false);

        action_menu_controller.tracker = MenuTracker.off;
        attack_menu_controller.tracker = MenuTracker.off;
        duet_menu_controller.tracker = MenuTracker.off;
        gambit_menu_controller.tracker = MenuTracker.off;
        item_menu_controller.tracker = MenuTracker.off;

        attack_menu_controller.index = 0;
        duet_menu_controller.index = 0;
        gambit_menu_controller.index = 0;
        item_menu_controller.index = 0;

        action_menu_controller.gameObject.SetActive(false);
        SetSubMenusActive(-1);

        CheckActionOptions();
    }

    public void SwitchToActionsMenu()
    {
        action_menu_controller.tracker = MenuTracker.on;
        attack_menu_controller.tracker = MenuTracker.off;
        duet_menu_controller.tracker = MenuTracker.off;
        gambit_menu_controller.tracker = MenuTracker.off;
        item_menu_controller.tracker = MenuTracker.off;

        SetSubMenusActive(-1);
        action_menu_animator.SetBool("OnMenu", false);

        attack_menu_controller.index = 0;
        duet_menu_controller.index = 0;
        gambit_menu_controller.index = 0;
        item_menu_controller.index = 0;

        SetCharacterColor(_character);
        description_box.HideBox();
        CheckActionOptions();
        action_menu_controller.gameObject.SetActive(true);

        _state = State.action;
        enabled = true;
    }

    public void SwitchToAttacksMenu()
    {
        action_menu_controller.tracker = MenuTracker.off;
        attack_menu_controller.tracker = MenuTracker.on;

        SetWhite();
        SetSubMenusActive(0);
        action_menu_animator.SetBool("OnMenu", true);
        previous_sub_menu_index = -1;

        CheckAttackOptions();
        _state = State.attacks;
    }

    public void SwitchToDuetsMenu()
    {
        action_menu_controller.tracker = MenuTracker.off;
        duet_menu_controller.tracker = MenuTracker.on;

        SetWhite();
        SetSubMenusActive(1);
        action_menu_animator.SetBool("OnMenu", true);
        previous_sub_menu_index = -1;

        CheckDuoOptions();
        _state = State.duets;
    }

    public void SwitchToGambitsMenu()
    {
        action_menu_controller.tracker = MenuTracker.off;
        gambit_menu_controller.tracker = MenuTracker.on;

        cycle_menu_object.SetActive(false);

        SetWhite();
        SetSubMenusActive(2);
        action_menu_animator.SetBool("OnMenu", true);
        previous_sub_menu_index = -1;

        CheckTacticOptions();
        _state = State.gambits;
    }

    public void SwitchToItemsMenu()
    {
        action_menu_controller.tracker = MenuTracker.off;
        item_menu_controller.tracker = MenuTracker.on;

        SetWhite();
        SetSubMenusActive(3);
        action_menu_animator.SetBool("OnMenu", true);
        previous_sub_menu_index = -1;

        CheckItemOptions();
        _state = State.items;
    }

    public void SwitchToCycleMenu()
    {
        gambit_menu_controller.tracker = MenuTracker.off;

        previous_state = _state;
        _state = State.cycle;

        cycle_menu_object.SetActive(true);
        cycle_menu.Set();
    }

    private void SetToSelector(int actor, Action.Side side, Action.Target target, int action, string name = "")
    {
        attack_menu_controller.tracker = MenuTracker.off;
        duet_menu_controller.tracker = MenuTracker.off;
        gambit_menu_controller.tracker = MenuTracker.off;
        item_menu_controller.tracker = MenuTracker.off;

        previous_state = _state;
        _state = State.selector;

        selector_system.AdjustIndices(actor, side, target, previous_state, action, name);
        selector_system.SetToOn();
    }

    private void CheckActionOptions()
    {
        // check initial menu
        if (AttackMenuOptions.Count == 0 && ActionMenuOptionAnimators[0].isActiveAndEnabled)
        {
            ActionMenuOptionAnimators[0].SetBool("Greyed", true);
        }

        if (DuetMenuOptions.Count == 0 && ActionMenuOptionAnimators[1].isActiveAndEnabled)
        {
            ActionMenuOptionAnimators[1].SetBool("Greyed", true);
        }

        if (GambitMenuOptions.Count == 0 && ActionMenuOptionAnimators[2].isActiveAndEnabled)
        {
            ActionMenuOptionAnimators[2].SetBool("Greyed", true);
        }

        if (ItemMenuOptions.Count == 0 && ActionMenuOptionAnimators[3].isActiveAndEnabled)
        {
            ActionMenuOptionAnimators[3].SetBool("Greyed", true);
        }
    }

    private void CheckAttackOptions()
    {
        for (int i = 0; i < AttackMenuOptions.Count; ++i)
        {
            AttackMenuOptions[i].Check();
        }
    }

    private void CheckDuoOptions()
    {
        for (int i = 0; i < DuetMenuOptions.Count; ++i)
        {
            DuetMenuOptions[i].Check();
        }
    }

    private void CheckTacticOptions()
    {
        for (int i = 0; i < GambitMenuOptions.Count; ++i)
        {
            GambitMenuOptions[i].Check();
        }
    }

    private void CheckItemOptions()
    {
        for (int i = 0; i < ItemMenuOptions.Count; ++i)
        {
            ItemMenuOptions[i].Check();
        }
    }

    public void SetActionMenuAnimator()
    {
        if (action_menu_controller.index == 0)
        {
            action_menu_animator.SetBool("OnAttacks", true);
            action_menu_animator.SetBool("OnDuets", false);
            action_menu_animator.SetBool("OnGambits", false);
            action_menu_animator.SetBool("OnItems", false);
        }
        else if (action_menu_controller.index == 1)
        {
            action_menu_animator.SetBool("OnAttacks", false);
            action_menu_animator.SetBool("OnDuets", true);
            action_menu_animator.SetBool("OnGambits", false);
            action_menu_animator.SetBool("OnItems", false);
        }
        else if (action_menu_controller.index == 2)
        {
            action_menu_animator.SetBool("OnAttacks", false);
            action_menu_animator.SetBool("OnDuets", false);
            action_menu_animator.SetBool("OnGambits", true);
            action_menu_animator.SetBool("OnItems", false);
        }
        else if (action_menu_controller.index == 3)
        {
            action_menu_animator.SetBool("OnAttacks", false);
            action_menu_animator.SetBool("OnDuets", false);
            action_menu_animator.SetBool("OnGambits", false);
            action_menu_animator.SetBool("OnItems", true);
        }
    }

    private void SetSubMenusActive(int index)
    {
        for (int i = 0; i < sub_menus.Length; ++i)
        {
            if (i == index)
            {
                sub_menus[i].SetActive(true);
            }
            else
            {
                sub_menus[i].SetActive(false);
            }
        }
    }

    public void SetAllCanSelect()
    {
        for (int i = 0; i < AttackMenuOptions.Count; ++i)
        {
            AttackMenuOptions[i].SetCanSelect(attached_character._energy, selector_system.battle_system.Enemies);
        }

        for (int i = 0; i < DuetMenuOptions.Count; ++i)
        {
            DuetMenuOptions[i].SetCanSelect(attached_character._energy, selector_system.battle_system.Enemies);
        }

        for (int i = 0; i < GambitMenuOptions.Count; ++i)
        {
            GambitMenuOptions[i].SetCanSelect(attached_character._energy, selector_system.battle_system.Enemies);
        }

        for (int i = 0; i < ItemMenuOptions.Count; ++i)
        {
            ItemMenuOptions[i].SetCanSelect(selector_system.battle_system.Enemies, selector_system.battle_system.Characters);
        }
    }

    public void UseItem(int index)
    {
        Destroy(ItemMenuOptions[index].gameObject);
        ItemMenuOptions.RemoveAt(index);

        for (int i = 0; i < ItemMenuOptions.Count; ++i)
        {
            ItemMenuOptions[i].GetComponent<MenuButtonAdvanced>().current_index = i;
            ItemMenuOptions[i].transform.localPosition = new Vector2(START_OFFSET_X, i * SUB_MENU_OFFSET + START_OFFSET_Y);
        }
        item_menu_controller.max_index = ItemMenuOptions.Count - 1;
        item_menu_controller.index = 0;
    }

    private void ReflectActionCost(int cost)
    {
        if (cost > 0)
        {
            if (player_hud._animator.GetBool("InFront"))
            {
                player_hud.ReflectCost(attached_character._index, attached_character._energy - cost);
            }
            else
            {
                player_hud.ReflectCost((attached_character._index + 1) % 2, attached_character._energy - cost);
            }
        }
    }

    private void SetStatToNormal()
    {
        if (player_hud._animator.GetBool("InFront"))
        {
            player_hud.ReflectNormal(attached_character._index, attached_character._energy);
        }
        else
        {
            player_hud.ReflectNormal((attached_character._index + 1) % 2, attached_character._energy);
        }
    }

    private void SetCharacterColor(string character)
    {
        foreach (var background in ActionMenuBackgrounds)
        {
            background.color = new Color(
                ConversationMenuItem.CharacterColors[character].r,
                ConversationMenuItem.CharacterColors[character].g,
                ConversationMenuItem.CharacterColors[character].b,
                background.color.a);
        }
    }

    private void SetWhite()
    {
        foreach (var background in ActionMenuBackgrounds)
        {
            background.color = new Color(1f, 1f, 1f, background.color.a);
        }
    }
}
