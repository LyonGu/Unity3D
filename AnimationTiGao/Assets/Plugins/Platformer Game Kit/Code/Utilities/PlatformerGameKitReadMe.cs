// Platformer Game Kit // https://kybernetik.com.au/platformer // Copyright 2021-2023 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace PlatformerGameKit
{
    /// <summary>[Editor-Only] A welcome screen for <see cref="PlatformerGameKit"/>.</summary>
    // [CreateAssetMenu]
    [HelpURL(Strings.APIDocumentation + "/" + nameof(PlatformerGameKitReadMe))]
    public class PlatformerGameKitReadMe : Animancer.Editor.ReadMe
    {
        /************************************************************************************************************************/

        /// <summary>The release ID of the current version.</summary>
        /// <example><list type="bullet">
        /// <item>[1] = v1.0: 2021-07-29.</item>
        /// <item>[2] = v1.1: 2023-04-01.</item>
        /// </list></example>
        protected override int ReleaseNumber => 2;

        /// <inheritdoc/>
        protected override string PrefKey => nameof(PlatformerGameKit);

        /// <inheritdoc/>
        protected override string BaseProductName => Strings.ProductName;

        /// <inheritdoc/>
        protected override string VersionName => "v1.1";

        /// <inheritdoc/>
        protected override string DocumentationURL => Strings.Documentation;

        /// <inheritdoc/>
        protected override string ChangeLogURL => Strings.ChangeLogPrefix;// + "/v1-1";

        /// <inheritdoc/>
        protected override string ExampleURL => Strings.Examples;

        /// <inheritdoc/>
        protected override string UpdateURL => Strings.LatestVersion;

        /************************************************************************************************************************/

        public PlatformerGameKitReadMe() : base(
            new LinkSection("Forum",
                "for general discussions, feedback, and news",
                Strings.Forum),
            new LinkSection("Issues",
                "for questions, suggestions, and bug reports",
                Strings.Issues),
            new LinkSection("Email",
                "for anything private",
                GetEmailURL(Strings.DeveloperEmail, Strings.ProductName),
                Strings.DeveloperEmail))
        { }

        /************************************************************************************************************************/
    }
}

#endif
