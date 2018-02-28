using System;
using Xamarin.Forms;

namespace eShopOnContainers.Core.Behaviors.Base {
    public class BindableBehavior<TBindable> : Behavior<TBindable> where TBindable : BindableObject {
        public TBindable AssociatedObject { get; private set; }

        protected override void OnAttachedTo(TBindable visualElement) {
            base.OnAttachedTo(visualElement);

            this.AssociatedObject = visualElement;

            if (visualElement.BindingContext != null)
                this.BindingContext = visualElement.BindingContext;

            visualElement.BindingContextChanged += this.OnBindingContextChanged;
        }

        protected virtual void OnBindingContextChanged(object sender, EventArgs e) {
            this.OnBindingContextChanged();
        }
        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            this.BindingContext = this.AssociatedObject.BindingContext;
        }

        protected override void OnDetachingFrom(TBindable view) {
            view.BindingContextChanged -= this.OnBindingContextChanged;
        }

    }
}
