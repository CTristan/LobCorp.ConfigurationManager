// SPDX-License-Identifier: LGPL-3.0-or-later

using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Disposable scope that suppresses config file saves until disposed.
    /// Created via <see cref="LmmConfigFile.BeginBatchSave"/>.
    /// </summary>
    public sealed class BatchSaveScope : IDisposable
    {
        private readonly LmmConfigFile _configFile;
        private bool _disposed;

        internal BatchSaveScope(LmmConfigFile configFile)
        {
            _configFile = configFile;
            _configFile.DisableSaving = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _configFile.DisableSaving = false;
            _configFile.Save();
        }
    }
}
