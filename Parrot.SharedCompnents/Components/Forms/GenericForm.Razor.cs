using Microsoft.AspNetCore.Components;
using Parrot.SharedComponents.Components.InputFields;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parrot.SharedComponents.Components.Forms
{
    public partial class GenericForm
    {
        [Parameter]
        public required List<FormInputModel> Fields { get; set; }
        [Parameter]
        public string SubmitButtonText { get; set; } = "Submit";
        [Parameter]
        public EventCallback<Dictionary<string, string>> OnSubmit { get; set; }

        protected async Task HandleSubmit()
        {
            var formData = new Dictionary<string, string>();
            foreach (var field in Fields)
            {
                if (!string.IsNullOrEmpty(field.Label))
                {
                    formData[field.Label] = field.Value;
                }
            }
            await OnSubmit.InvokeAsync(formData);
        }

        protected void UpdateFieldValue(string id, string fieldValue)
        {
            FormInputModel? field = Fields.Find(f => f.Id == id);
            if (field != null)
            {
                field.Value = fieldValue;
            }
        }
    }
}
