using System.Collections.Generic;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class ViewCollection : KeyCollection<View>
    {
        private readonly KeyboardLayout _layout;
        private readonly List<View> _views = new List<View>();

        internal ViewCollection(KeyboardLayout layout)
        {
            _layout = layout;
        }

        public View this[int index] => _views[index];

        public override void Clear()
        {
            base.Clear();

            _views.Clear();
        }

        public override void Add(View item)
        {
            base.Add(item);

            _views.Add(item);
            foreach (var keyRef in item.KeyRefs)
            {
                keyRef.Attach(_layout);
            }
        }

        public override bool Remove(View item)
        {
            var value = base.Remove(item);
            _views.Remove(item);
            return value;
        }
    }
}