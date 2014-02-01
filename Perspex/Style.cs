﻿// -----------------------------------------------------------------------
// <copyright file="Style.cs" company="Tricycle">
// Copyright 2014 Tricycle. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Perspex.Controls;

    public class Style
    {
        private bool applied;

        public Style()
        {
            this.Setters = new List<Setter>();
        }

        public Func<Control, Match> Selector
        {
            get;
            set;
        }

        public IEnumerable<Setter> Setters
        {
            get;
            set;
        }

        public void Attach(Control control)
        {
            Match match = this.Selector(control);

            if (match != null)
            {
                List<IObservable<bool>> o = new List<IObservable<bool>>();
                
                while (match != null)
                {
                    if (match.Observable != null)
                    {
                        o.Add(match.Observable);
                    }

                    match = match.Previous;
                }

                Observable.CombineLatest(o).Subscribe(x =>
                {
                    if (x.All(y => y))
                    {
                        this.Apply(control);
                    }
                    else if (this.applied)
                    {
                        this.Unapply(control);
                    }
                });
            }
        }
        
        private void Apply(Control control)
        {
            if (this.Setters != null)
            {
                foreach (Setter setter in this.Setters)
                {
                    setter.Apply(control);
                }
            }

            this.applied = true;
        }

        private void Unapply(Control control)
        {
            if (this.Setters != null)
            {
                foreach (Setter setter in this.Setters)
                {
                    setter.Unapply(control);
                }

                this.applied = false;
            }
        }
    }
}