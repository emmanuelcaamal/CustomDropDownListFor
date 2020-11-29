using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace FinancialControl.Web.Common.Extensions
{
    public static class DropDownCustomHelper
    {
        public static IHtmlContent DropDownListWithTagFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, string optionLabel, dynamic list,
            DropDownCutomValues listOptions, string selectedValue, object htmlAttributes)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;
            var modelExpression = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);
            string id = modelExpression.Name;
            return DropDownDataTag(htmlHelper, id, modelExpression.ModelExplorer, optionLabel, list, listOptions, selectedValue, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private static IHtmlContent DropDownDataTag(this IHtmlHelper htmlHelper, string id, ModelExplorer modelExplorer, string optionLabel, dynamic list,
            DropDownCutomValues listOptions, string selectedValue, object htmlAttributes)
        {
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(id);
            if (String.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("id");
            }

            string htmlOutput = "";
            TagBuilder dropdown = new TagBuilder("select");
            dropdown.Attributes.Add("name", id);
            dropdown.Attributes.Add("id", id);
            dropdown.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            StringBuilder options = new StringBuilder();

            // Make optionLabel the first item that gets rendered.
            if (optionLabel != null)
                options.Append("<option value='" + String.Empty + "'>" + optionLabel + "</option>");

            foreach (var item in list)
            {
                string dataAttributes = string.Empty;
                string selectedOption = string.Empty;

                if (!string.IsNullOrEmpty(listOptions.DataAttributes))
                {
                    var dataAttributeSplit = listOptions.DataAttributes.Split(',');
                    if (dataAttributeSplit.Length > 0)
                    {
                        foreach (var dataAttribute in dataAttributeSplit)
                        {
                            dataAttributes += string.Format("data-{0}='{1}' ", dataAttribute.ToLower().Trim(), DynamicHelper.GetProperty(item, dataAttribute.Trim()));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(selectedValue))
                {
                    if (DynamicHelper.GetProperty(item, listOptions.Value) == selectedValue)
                    {
                        selectedOption = "selected='selected'";
                    }
                }

                options.Append("<option value='" + DynamicHelper.GetProperty(item, listOptions.Value) + "' " + dataAttributes + " " + selectedOption + ">" + DynamicHelper.GetProperty(item, listOptions.Text) + "</option>");
            }

            dropdown.InnerHtml.AppendHtml(options.ToString());

            if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out var modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    dropdown.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }
            var validator = htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(ValidationHtmlAttributeProvider)) as ValidationHtmlAttributeProvider;
            validator.AddAndTrackValidationAttributes(htmlHelper.ViewContext, modelExplorer, id, dropdown.Attributes);

            using (var writer = new StringWriter())
            {
                dropdown.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                htmlOutput = writer.ToString();
            }
            return new HtmlString(htmlOutput);
        }
    }

    public class DropDownCutomValues
    {
        /// <summary>
        /// Valor que será seleccionado
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Valor que será mostrado como opción de selección
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Valores que se agregaran como opción adicional
        /// </summary>
        public string DataAttributes { get; set; }
    }
}
