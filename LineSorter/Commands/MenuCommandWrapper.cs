﻿using EnvDTE;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using System.Linq;

namespace LineSorter.Commands
{
    public class MenuCommandWrapper
    {
        #region Var
        public string Text { get; }
        public EventHandler InvokeHandler { get; }
        public EventHandler ChangeHandler { get; }
        public OleMenuCommand Cmd { get; private set; }
        public CommandID CommandID { get; private set; }
        public EventHandler BeforeQueryStatusHandler { get; }
        private Dictionary<string, object> Parameters { get; }
        #endregion

        #region Init
        private MenuCommandWrapper(OleMenuCommand Cmd, EventHandler InvokeHandler, EventHandler ChangeHandler, EventHandler BeforeQueryStatusHandler, CommandID CommandID, string Text)
        {
            this.Cmd = Cmd;
            this.Text = Text;
            this.CommandID = CommandID;
            this.ChangeHandler = ChangeHandler;
            this.InvokeHandler = InvokeHandler;
            this.BeforeQueryStatusHandler = BeforeQueryStatusHandler;
            Parameters = new Dictionary<string, object>();
        }
        #endregion

        #region Functions
        public static implicit operator OleMenuCommand(MenuCommandWrapper Wrapper) => Wrapper.Cmd;

        public static MenuCommandWrapper Create(EventHandler InvokeHandler, EventHandler ChangeHandler, EventHandler BeforeQueryStatusHandler, CommandID CommandID, Predicate<int> Match, string Text)
        {
            BeforeQueryStatusHandler = BeforeQueryStatusHandler ?? ((sender, e) => {
                OleMenuCommand current = (OleMenuCommand)sender;
                current.Text = Text;
                current.Supported = true;
                current.Visible = true;
                current.Enabled = true;
                current.MatchedCommandId = current.CommandID.ID;
            });
            DynamicCommand cmd = new DynamicCommand(InvokeHandler, ChangeHandler, BeforeQueryStatusHandler, CommandID, Match);
            return new MenuCommandWrapper(cmd, InvokeHandler, ChangeHandler, BeforeQueryStatusHandler, CommandID, Text);
        }
        public static MenuCommandWrapper Create(EventHandler InvokeHandler, EventHandler ChangeHandler, EventHandler BeforeQueryStatusHandler, CommandID CommandID, string Text)
        {
            BeforeQueryStatusHandler = BeforeQueryStatusHandler ?? ((sender, e) => {
                OleMenuCommand current = (OleMenuCommand)sender;
                current.Text = Text;
                current.Supported = true;
                current.Visible = true;
                current.Enabled = true;
                current.MatchedCommandId = 0;
            });
            OleMenuCommand cmd = new OleMenuCommand(InvokeHandler, ChangeHandler, BeforeQueryStatusHandler, CommandID);
            return new MenuCommandWrapper(cmd, InvokeHandler, ChangeHandler, BeforeQueryStatusHandler, CommandID, Text);
        }
        public static MenuCommandWrapper Create(EventHandler InvokeHandler, CommandID CommandID, string Text) => Create(InvokeHandler, null, null, CommandID, Text);
        public static MenuCommandWrapper Create(EventHandler InvokeHandler, CommandID CommandID, Predicate<int> Match, string Text) => Create(InvokeHandler, null, null, CommandID, Match, Text);

        public object this[string Key]
        {
            get => Parameters[Key];
            set => Parameters[Key] = value;
        }
        public void SetParameter<T>(string Key, T Value) => this[Key] = Value;
        public T GetParameter<T>(string Key) => Parameters.ContainsKey(Key) ? (T)this[Key] : default(T);

        public void ChangeCmdID(CommandID NewID)
        {
            Cmd = Create(InvokeHandler, ChangeHandler, BeforeQueryStatusHandler, NewID, Text).Cmd;
            CommandID = NewID;
        }
        public void ChangeCmdID(int NewID) => ChangeCmdID(new CommandID(CommandID.Guid, NewID));

        public async Task SetHotkeyAsync(Keys Mod1, Keys Key1, Keys Key2) => await SetBindingsAsync(GetStringBinding(Mod1, Key1, Key2));
        public async Task SetHotkeyAsync(Keys Mod1, Keys Key1, Keys Mod2, Keys Key2) => await SetBindingsAsync(GetStringBinding(Mod1, Key1, Mod2, Key2));
        public async Task RemoveHotkeyAsync() => await SetBindingsAsync();
        private async Task SetBindingsAsync(params string[] Bindings)
        {
            await VSPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = (await VSPackage.Instance.GetServiceAsync(typeof(DTE))) as DTE;
            Command cmd = dte.Commands.Item(CommandID.Guid.ToString(), CommandID.ID);
            cmd.Bindings = Bindings;
        }
        private string GetStringBinding(params Keys[] Keys)
        {
            return $"Global::{string.Join("+", Keys.Select(getKey))}";
            string getKey(Keys key)
            {
                if ((int)key > 47 && (int)key < 58)
                    return ((int)key - 48).ToString();
                return key.ToString();
            }
        }

        public override string ToString() => Text;
        public override int GetHashCode() => CommandID.GetHashCode();
        public override bool Equals(object obj) => obj is MenuCommandWrapper wrapper && wrapper.CommandID == CommandID;
        #endregion
    }
}
