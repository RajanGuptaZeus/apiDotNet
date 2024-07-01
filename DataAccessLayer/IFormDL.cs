using FileUploadApp.CommonLayer;


namespace FileUploadApp.DataAccessLayer
{
    public interface IFormDL
    {
        public Task<FormResponse> FormValue (FormRequest request);
        public Task<List<String>> GetFormValues ();
        public Task<String> DeleteUser (int id);
        public Task<String> UpdateUserById (int id , FormRequest request);
    }
}