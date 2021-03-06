﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;

namespace Avalonia.Controls
{
    /// <summary>
    /// Initializes up platform-specific services for an <see cref="Application"/>.
    /// </summary>
    public class AppBuilder
    {
        /// <summary>
        /// Gets or sets the <see cref="Application"/> instance being initialized.
        /// </summary>
        public Application Instance { get; set; }

        /// <summary>
        /// Gets or sets a method to call the initialize the windowing subsystem.
        /// </summary>
        public Action WindowingSubsystem { get; set; }

        /// <summary>
        /// Gets or sets a method to call the initialize the windowing subsystem.
        /// </summary>
        public Action RenderingSubsystem { get; set; }

        /// <summary>
        /// Gets or sets a method to call before <see cref="Start{TMainWindow}"/> is called on the
        /// <see cref="Application"/>.
        /// </summary>
        public Action<AppBuilder> BeforeStartCallback { get; set; }

        /// <summary>
        /// Begin configuring an <see cref="Application"/>.
        /// </summary>
        /// <typeparam name="TApp">The subclass of <see cref="Application"/> to configure.</typeparam>
        /// <returns>An <see cref="AppBuilder"/> instance.</returns>
        public static AppBuilder Configure<TApp>()
            where TApp : Application, new()
        {
            return Configure(new TApp());
        }

        /// <summary>
        /// Begin configuring an <see cref="Application"/>.
        /// </summary>
        /// <returns>An <see cref="AppBuilder"/> instance.</returns>
        public static AppBuilder Configure(Application app)
        {
            AvaloniaLocator.CurrentMutable.BindToSelf(app);

            return new AppBuilder()
            {
                Instance = app,
            };
        }

        /// <summary>
        /// Registers a callback to call before <see cref="Start{TMainWindow}"/> is called on the
        /// <see cref="Application"/>.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns>An <see cref="AppBuilder"/> instance.</returns>
        public AppBuilder BeforeStarting(Action<AppBuilder> callback)
        {
            BeforeStartCallback = callback;
            return this;
        }

        /// <summary>
        /// Starts the application with an instance of <typeparamref name="TMainWindow"/>.
        /// </summary>
        /// <typeparam name="TMainWindow">The window type.</typeparam>
        public void Start<TMainWindow>()
            where TMainWindow : Window, new()
        {
            Setup();
            BeforeStartCallback?.Invoke(this);

            var window = new TMainWindow();
            window.Show();
            Instance.Run(window);
        }

        /// <summary>
        /// Sets up the platform-specific services for the application, but does not run it.
        /// </summary>
        /// <returns></returns>
        public AppBuilder SetupWithoutStarting()
        {
            Setup();
            return this;
        }

        /// <summary>
        /// Specifies a windowing subsystem to use.
        /// </summary>
        /// <param name="initializer">The method to call to initialize the windowing subsystem.</param>
        /// <returns>An <see cref="AppBuilder"/> instance.</returns>
        public AppBuilder WithWindowingSubsystem(Action initializer)
        {
            WindowingSubsystem = initializer;
            return this;
        }

        /// <summary>
        /// Specifies a rendering subsystem to use.
        /// </summary>
        /// <param name="initializer">The method to call to initialize the rendering subsystem.</param>
        /// <returns>An <see cref="AppBuilder"/> instance.</returns>
        public AppBuilder WithRenderingSubsystem(Action initializer)
        {
            RenderingSubsystem = initializer;
            return this;
        }

        /// <summary>
        /// Sets up the platform-speciic services for the <see cref="Application"/>.
        /// </summary>
        private void Setup()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("No App instance configured.");
            }

            if (WindowingSubsystem == null)
            {
                throw new InvalidOperationException("No windowing system configured.");
            }

            if (RenderingSubsystem == null)
            {
                throw new InvalidOperationException("No rendering system configured.");
            }

            Instance.RegisterServices();
            WindowingSubsystem();
            RenderingSubsystem();
            Instance.Initialize();
        }
    }
}
