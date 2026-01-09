using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalCap.Core.Models.Skylight
{
    public class Component
    {
        [JsonProperty(PropertyName = "componentType")]
        [Required(AllowEmptyStrings = false)]
        public virtual ComponentType ComponentType { get; set; }
        [JsonProperty(PropertyName = "done")]
        public Done Done { get; set; }
        [JsonProperty(PropertyName = "sequenceId")]
        public string SequenceId { get; set; }
        public Dictionary<string, Choice> Choices { get; set; }
        public ComponentDecisionLink Link { get; set; }
        public Action DoneAction { get; set; }
        public bool? IncludeCapture { get; set; }
        public int? MaxSelected { get; set; }
        public List<string> Captures { get; set; } = new List<string>();
        public List<ComponentDecisionCaptures> DecisionCaptures { get; set; }
        public int? MinSelected { get; set; }
        public bool? Mutable { get; set; }
        public int? Delay { get; set; }


    }
    public enum ComponentType
    {
        Default = 0,
        Calling = 1,
        CapturePhoto = 2,
        CaptureVideo = 3,
        CaptureAudio = 4,
        Completion = 5,
        Scanning = 6,
        Decision = 7,
        OpenSequence = 8
    }
    public class ComponentDefault : Component
    {

        [JsonProperty(PropertyName = "componentType")]
        [Required(AllowEmptyStrings = false)]
        public override ComponentType ComponentType { get; set; }
        [JsonProperty(PropertyName = "done")]
        public Done Done { get; set; }
        [JsonProperty(PropertyName = "doneAction")]
        public Action DoneAction { get; set; }
    }
    public class ComponentCompletion : Component
    {

        [JsonProperty(PropertyName = "componentType")]
        [Required(AllowEmptyStrings = false)]
        public override ComponentType ComponentType { get; set; }
        [JsonProperty(PropertyName = "completed")]
        public bool? Completed { get; set; }
        [JsonProperty(PropertyName = "completionTime")]
        public string CompletionTime { get; set; }
        [JsonProperty(PropertyName = "done")]
        public Done Done { get; set; }
        [JsonProperty(PropertyName = "doneAction")]
        public Action DoneAction { get; set; }
        [JsonProperty(PropertyName = "moveDelay")]
        public int? MoveDelay { get; set; }
        [JsonProperty(PropertyName = "returnPrev")]
        public bool? ReturnPrev { get; set; }
        [JsonProperty(PropertyName = "scrollLock")]
        public bool? ScrollLock { get; set; }
    }
    //public class ComponentOpenSequence : Component
    //{

    //}
    public class Layout
    {

        [JsonProperty(PropertyName = "layoutType")]
        [Required(AllowEmptyStrings = false)]
        public virtual LayoutType LayoutType { get; set; }

        public LayoutTextInputFormat Format { get; set; }
        public string HelpText { get; set; }
        public string Input { get; set; }
        public bool? Mutable { get; set; }
        public string Placeholder { get; set; }
        public bool? ShowIcon { get; set; }

        [JsonProperty(PropertyName = "alignment")]
        public Alignment Alignment { get; set; }
        [JsonProperty(PropertyName = "autoFit")]
        public bool? AutoFit { get; set; }
        [JsonProperty(PropertyName = "bgColor")]
        public string BgColor { get; set; }
        [JsonProperty(PropertyName = "fgColor")]
        public string FgColor { get; set; }
        [JsonProperty(PropertyName = "text")]
        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "textSize")]
        [Required(AllowEmptyStrings = false)]
        public TextSize TextSize { get; set; }

    }

    public enum LayoutType
    {
        Text = 0,
        Web = 1,
        TextInput = 2,
        Image = 3,
        AvMedia = 4,
        Pdf = 5,
        Svg = 6,
        Dynamic = 7
    }
    public enum LayoutTextInputFormatType
    {
        Generic = 0,
        Number = 1
    }
    public class LayoutTextInputFormat
    {
        [JsonProperty(PropertyName = "type")]
        public LayoutTextInputFormatType Type { get; set; }

        public int? MaxChar { get; set; }

    }
    public class LayoutTextInput : Layout
    {
        public override LayoutType LayoutType { get; set; }
        public LayoutTextInputFormat Format { get; set; }
        public string HelpText { get; set; }
        public string Input { get; set; }
        public bool? Mutable { get; set; }
        public string Placeholder { get; set; }
        public bool? ShowIcon { get; set; }
    }

    public enum Alignment
    {
        Center = 0,
        Left = 1,
        Right = 2
    }
    public enum TextSize
    {
        Small = 0,
        Large = 1
    }
    public class LayoutText : Layout
    {

        [JsonProperty(PropertyName = "layoutType")]
        [Required(AllowEmptyStrings = false)]
        public override LayoutType LayoutType { get; set; }
        [JsonProperty(PropertyName = "alignment")]
        public Alignment Alignment { get; set; }
        [JsonProperty(PropertyName = "autoFit")]
        public bool? AutoFit { get; set; }
        [JsonProperty(PropertyName = "bgColor")]
        public string BgColor { get; set; }
        [JsonProperty(PropertyName = "fgColor")]
        public string FgColor { get; set; }
        [JsonProperty(PropertyName = "text")]
        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "textSize")]
        [Required(AllowEmptyStrings = false)]
        public TextSize TextSize { get; set; }

    }
    public enum DoneType
    {
        CallConnected = 0,
        OnFocus = 1,
        OnSelect = 2,
        DataCommitted = 3,
        MediaWatched = 4,
        MinCaptured = 5,
        MinChoices = 6,
        ScanSuccess = 7
    }
    public class Done
    {
        public virtual DoneType Type { get; set; }
    }
    public class ComponentCapturePhoto : Component
    {
        public override ComponentType ComponentType { get; set; }
        public List<string> Captures { get; set; }
        public int? Delay { get; set; }
        public Done Done { get; set; }
        public Action DoneAction { get; set; }
    }
    public class ComponentDecisionLink
    {

        [JsonProperty(PropertyName = "text")]
        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "sequenceId")]
        [Required(AllowEmptyStrings = false)]
        public string SequenceId { get; set; }
        [JsonProperty(PropertyName = "cardId")]
        public string CardId { get; set; }
    }
    public class Choice
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }
        [JsonProperty(PropertyName = "position")]
        public decimal? Position { get; set; }
        [JsonProperty(PropertyName = "selected")]
        public bool? Selected { get; set; }
        [JsonProperty(PropertyName = "action")]
        public Action Action { get; set; }
    }

    public class ComponentDecision : Component
    {
        public override ComponentType ComponentType { get; set; }
        public Dictionary<string, Choice> Choices { get; set; }
        public ComponentDecisionLink Link { get; set; }
        public Done Done { get; set; }
        public Action DoneAction { get; set; }
        public bool? IncludeCapture { get; set; }
        public int? MaxSelected { get; set; }
        public List<ComponentDecisionCaptures> ComponentDecisionCaptures { get; set; }
        public int? MinSelected { get; set; }
        public bool? Mutable { get; set; }


    }
    public class ComponentDecisionCaptures
    {
        public string Id { get; set; }
        public string MimeType { get; set; }
    }

    //public class Card
    //{
    //    public string id { get; set; }
    //    public string label { get; set; }
    //    public bool isAttached { get; set; }
    //    public int size { get; set; }
    //    public bool hideLabel { get; set; }
    //    public bool required { get; set; }
    //    public Component component { get; set; }
    //    public int position { get; set; }
    //    public Layout layout { get; set; }
    //    public bool selectable { get; set; }
    //}
    public class Card
    {
        public string Label { get; set; }
        public string Updated { get; set; }
        public string TemplateId { get; set; }
        public List<string> Tags { get; set; }
        public bool? Subdued { get; set; }
        public int? Size { get; set; }
        public string SequenceId { get; set; }
        public bool? Selectable { get; set; }
        public int? Revision { get; set; }
        public bool? Required { get; set; }
        public decimal? Position { get; set; }
        public List<Guid?> Notes { get; set; }
        public bool? Locked { get; set; }
        public Layout Layout { get; set; }
        public List<string> Voice { get; set; }
        public bool? IsDone { get; set; }
        public string IntegrationId { get; set; }
        public string Id { get; set; }
        public bool? HideLabel { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
        public Component Component { get; set; }
        public string AssignmentId { get; set; }
        public string UpdatedBy { get; set; }
    }
}