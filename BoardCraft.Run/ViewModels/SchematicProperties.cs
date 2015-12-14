namespace BoardCraft.Run.ViewModels
{
    using System.ComponentModel;

    class SchematicProperties
    {
        [Category("Information")]
        [DisplayName("Component Count")]
        [Description("This property uses a TextBox as the default editor.")]
        public int ComponentCount { get { return 50; } }
    }
}
