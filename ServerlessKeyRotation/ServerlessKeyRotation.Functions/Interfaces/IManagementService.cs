using ServerlessKeyRotation.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessKeyRotation.Functions.Interfaces
{
    public interface IManagementService
    {
        public Task<bool> RotateStorageKeyForAppServiceAsync(RotationKeysConfiguration rotationConfig);
    }
}
