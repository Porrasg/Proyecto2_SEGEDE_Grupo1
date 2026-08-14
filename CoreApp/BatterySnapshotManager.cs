using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    public class BatterySnapshotManager
    {
        public List<BatterySnapshot> RetrieveAllSnapshots()
        {
            var snapshotCrud =
                new BatterySnapshotCrudFactory();

            return snapshotCrud.RetrieveAll<BatterySnapshot>();
        }
    }
}
