// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Perspex.Data;
using Perspex.Markup.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Perspex.Markup.UnitTests.Data
{
    public class ExpressionObserverTests_Validation
    {
        [Fact]
        public void Exception_Validation_Sends_ValidationUpdate()
        {
            var data = new ExceptionTest { MustBePositive = 5 };
            var observer = new ExpressionObserver(data, nameof(data.MustBePositive), ValidationMethods.Exceptions);
            var validationMessageFound = false;
            observer.Where(o => o is IValidationStatus).Subscribe(_ => validationMessageFound = true);
            observer.SetValue(-5);
            Assert.True(validationMessageFound);
        }

        [Fact]
        public void Disabled_Exception_Validation_Does_Not_Send_ValidationUpdate()
        {
            var data = new ExceptionTest { MustBePositive = 5 };
            var observer = new ExpressionObserver(data, nameof(data.MustBePositive), ValidationMethods.None);
            var validationMessageFound = false;
            observer.Where(o => o is IValidationStatus).Subscribe(_ => validationMessageFound = true);
            Assert.Throws<TargetInvocationException>(() => observer.SetValue(-5));
            Assert.False(validationMessageFound);
        }

        [Fact]
        public void Disabled_Indei_Validation_Does_Not_Subscribe()
        {
            var data = new IndeiTest { MustBePositive = 5 };
            var observer = new ExpressionObserver(data, nameof(data.MustBePositive), ValidationMethods.None);

            observer.Subscribe(_ => { });

            Assert.Equal(0, data.SubscriptionCount);
        }

        [Fact]
        public void Enabled_Indei_Validation_Subscribes()
        {
            var data = new IndeiTest { MustBePositive = 5 };
            var observer = new ExpressionObserver(data, nameof(data.MustBePositive), ValidationMethods.INotifyDataErrorInfo);
            var sub = observer.Subscribe(_ => { });

            Assert.Equal(1, data.SubscriptionCount);
            sub.Dispose();
            Assert.Equal(0, data.SubscriptionCount);
        }

        public class ExceptionTest : INotifyPropertyChanged
        {
            private int _mustBePositive;

            public int MustBePositive
            {
                get { return _mustBePositive; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                    _mustBePositive = value;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private class IndeiTest : INotifyDataErrorInfo
        {
            private int _mustBePositive;
            private Dictionary<string, IList<string>> _errors = new Dictionary<string, IList<string>>();
            private EventHandler<DataErrorsChangedEventArgs> _errorsChanged;

            public int MustBePositive
            {
                get { return _mustBePositive; }
                set
                {
                    if (value >= 0)
                    {
                        _mustBePositive = value;
                        _errors.Remove(nameof(MustBePositive));
                        _errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(MustBePositive)));
                    }
                    else
                    {
                        _errors[nameof(MustBePositive)] = new[] { "Must be positive" };
                        _errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(MustBePositive)));
                    }
                }
            }

            public bool HasErrors => _mustBePositive >= 0;

            public int SubscriptionCount { get; private set; }

            public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
            {
                add
                {
                    _errorsChanged += value;
                    ++SubscriptionCount;
                }
                remove
                {
                    _errorsChanged -= value;
                    --SubscriptionCount;
                }
            }

            public IEnumerable GetErrors(string propertyName)
            {
                IList<string> result;
                _errors.TryGetValue(propertyName, out result);
                return result;
            }
        }

    }
}
