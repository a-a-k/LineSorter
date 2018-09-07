using EnvDTE;
using System;
using System.Threading;
using LineSorter.Helpers;
using LineSorter.Commands;
using Microsoft.VisualStudio;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;

namespace LineSorter
{
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideBindingPath(SubPath = "UserSort")]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(Options.OptionPageGrid), "LineSorter", "General", 0, 0, true, 0)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.FirstLaunchSetup_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.BackgroundProjectLoad_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : AsyncPackage
    {
        #region Var
        public static string Path { get; }
        public static SortsLoader Loader { get; }
        public static string DllLocation { get; }
        public static Guid Guid { get; } = new Guid(PackageGuidString);
        public const string PackageGuidString = "7fb18e2a-1a51-4dbb-b676-a3514e44823d";
        #endregion

        #region Init
        static VSPackage()
        {
            string assemblyPath = new Uri(typeof(VSPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath;
            int index = assemblyPath.LastIndexOf('\\');
            if (index == -1)
                index = assemblyPath.LastIndexOf('/');
            DllLocation = assemblyPath;
            Path = assemblyPath.Substring(0, index + 1);
            Loader = new SortsLoader();
        }
        #endregion

        #region Functions
        protected override async Task InitializeAsync(CancellationToken CancellationToken, IProgress<ServiceProgressData> Progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken);

            await CommandLengthSort.InitializeAsync(this);
            await CommandLengthSortDesc.InitializeAsync(this);
            await CommandAlphSort.InitializeAsync(this);
            await CommandAlphSortDesc.InitializeAsync(this);
            await CommandRandomSort.InitializeAsync(this);
            await CommandUserSort.InitializeAsync(this);
            await CommandAnchor.InitializeAsync(this);
        }
        #endregion
    }
}
