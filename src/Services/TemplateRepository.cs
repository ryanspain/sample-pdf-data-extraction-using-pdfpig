using System.Text.Json;
using Blazored.LocalStorage;

public class TemplateRepository(ILocalStorageService LocalStorage)
{
    public async Task<List<Template>> GetAll()
    {
        // Retrieve existing templates from local storage
        var existingTemplatesJson = await LocalStorage.GetItemAsync<string>("templates");

        // Deserialize existing templates or initialize a new list
        if (string.IsNullOrWhiteSpace(existingTemplatesJson)) return [];
    
        // Attempt to deserialize the JSON string into an array of Template objects
        try
        {
            return JsonSerializer.Deserialize<List<Template>>(existingTemplatesJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<Template?> Get(string id)
    {
        // Retrieve existing templates from local storage
        var templates = await GetAll();

        // Check if the template already exists in the list
        var existingTemplateIndex = templates.FindIndex(t => t.Id == id);

        // If the template already exists, update it; otherwise, add it
        if (existingTemplateIndex >= 0)
            return templates[existingTemplateIndex];
        
        return null;
    }

    public async Task<Template> Save(Template item)
    {
        // Retrieve existing templates from local storage
        var templates = await GetAll();

        // Check if the template already exists in the list
        var existingTemplateIndex = templates.FindIndex(t => t.Id == item.Id);

        // If the template already exists, update it; otherwise, add it
        if (existingTemplateIndex >= 0)
            templates[existingTemplateIndex] = item;
        else
            templates.Add(item);

        // Save the updated list back to local storage
        await LocalStorage.SetItemAsync("templates", JsonSerializer.Serialize(templates));

        // Return the saved item
        return item;
    }

    public async Task Delete(string id)
    {
        // Retrieve existing templates from local storage
        var templates = await GetAll();
        
        // Find the index of the template to delete
        var indexToDelete = templates.FindIndex(t => t.Id == id);
        
        // If the template exists, remove it from the list
        if (indexToDelete >= 0)
            templates.RemoveAt(indexToDelete);
        
        // Save the updated list back to local storage
        await LocalStorage.SetItemAsync("templates", JsonSerializer.Serialize(templates));
    }
}