﻿using System;
using System.Numerics;
using DeathRoll.Data;
using DeathRoll.Gui;
using DeathRoll.Logic;
using ImGuiNET;

namespace DeathRoll;

// It is good to have this be disposable in general, in case you ever need it
// to do any cleanup
public class PluginUI : IDisposable
{
    public Participants Participants;
    public Configuration Configuration;
    public Rolls Rolls;

    private bool settingsVisible;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible;

    
    public PluginUI(Plugin plugin, Configuration configuration, Participants participants, Rolls rolls)
    {
        this.Configuration = configuration;
        this.Participants = participants;
        this.Rolls = rolls;
        
        Blocklist = new Blocklist(configuration);
        GeneralSettings = new GeneralSettings(this, configuration);
        Highlights = new Highlights(this);
        RollTable = new RollTable(this);
        DeathRollMode = new DeathRollMode(this);
        SimpleTournamentMode = new SimpleTournamentMode(this);
        BlackjackMode = new BlackjackMode(plugin, this);

        // needs RollTable
        TimerSetting = new TimerSetting(configuration, RollTable);
    }

    private Blocklist Blocklist { get; }
    private GeneralSettings GeneralSettings { get; }
    private Highlights Highlights { get; }
    public RollTable RollTable { get; }
    public DeathRollMode DeathRollMode { get; }
    public SimpleTournamentMode SimpleTournamentMode { get; }
    public BlackjackMode BlackjackMode { get; }
    public TimerSetting TimerSetting { get; }

    public bool Visible
    {
        get => visible;
        set => visible = value;
    }

    public bool SettingsVisible
    {
        get => settingsVisible;
        set => settingsVisible = value;
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        // This is our only draw handler attached to UIBuilder, so it needs to be
        // able to draw any windows we might have open.
        // Each method checks its own visibility/state to ensure it only draws when
        // it actually makes sense.
        // There are other ways to do this, but it is generally best to keep the number of
        // draw delegates as low as possible.

        DrawMainWindow();
        DrawSettingsWindow();
        SimpleTournamentMode.DrawGeneratedBracket();
        SimpleTournamentMode.DrawMatch();
        BlackjackMode.DrawGameField();
    }

    public void DrawMainWindow()
    {
        if (!Visible) return;

        ImGui.SetNextWindowSize(new Vector2(375, 480), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(375, 480), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("DeathRoll Helper", ref visible))
        {
            switch (Configuration.GameMode)
            {
                case GameModes.Venue:
                    RollTable.MainRender();
                    break;
                case GameModes.DeathRoll:
                    DeathRollMode.MainRender();
                    break;
                case GameModes.Tournament:
                    SimpleTournamentMode.MainRender();
                    break;
                case GameModes.Blackjack:
                    BlackjackMode.MainRender();
                    break;
                default:
                    ImGui.Text("Not Implemented!");
                    break;
            }
        }

        ImGui.End();
    }

    public void DrawSettingsWindow()
    {
        if (!SettingsVisible) return;

        ImGui.SetNextWindowSize(new Vector2(330, 530), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(330, 530), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("DRH Settings", ref settingsVisible))
            if (ImGui.BeginTabBar("##settings-tabs"))
            {
                // Renders General Settings UI
                GeneralSettings.RenderGeneralSettings();

                // Renders Timer Settings Tab
                TimerSetting.RenderTimerSettings();

                // Renders Highlight Settings Tab
                Highlights.RenderHightlightsTab();

                // Renders Blocklist Tab
                Blocklist.RenderBlocklistTab();
                
                // Renders Debug Tab
                GeneralSettings.RenderDebugTab();
                
                ImGui.EndTabBar();
            }

        ImGui.End();
    }
}