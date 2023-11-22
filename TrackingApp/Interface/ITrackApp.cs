using TrackingApp.DTO;
using TrackingApp.Entities;

namespace TrackingApp.Interface
{
    public interface ITrackApp
    {
        public bool ImportExcelData(string path);

        public bool AddOrUpdateViaForm(TrackingDTO trackingDTO);
        // bool UpdateRecord(string email, TrackingDB trackingDB);
        bool UpdateRecord(string email, TrackingDTO trackingDTO);
        bool DeleteRecord(string email);
     // List<TrackingDB> GetRecords();
        List<TrackingDB> ShowRecordByStatus(string status);
        TrackingDB ShowRecordByEmail(string email);
    }
}
