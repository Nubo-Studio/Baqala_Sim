using UnityEngine;
using Core.Events;

namespace Player.States
{
    public abstract class InteractionState
    {
        protected Interactor ctx;
        protected InteractionEvents events;

        public InteractionState(Interactor context, InteractionEvents events)
        {
            this.ctx = context;
            this.events = events;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void HandleInput();
        public abstract void Exit();
    }
}