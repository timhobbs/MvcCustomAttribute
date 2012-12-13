using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace MvcCustomAttribute.Models {

    /// <summary>
    /// Summary description for ShipRate
    /// </summary>
    public class ShipRate {

        public IList<SelectListItem> Carriers { get; set; }

        public IList<SelectListItem> ShipMethods { get; set; }

        [Required(ErrorMessage = "Carrier required")]
        public string Carrier { get; set; }

        [Required(ErrorMessage = "Ship method required")]
        public string ShipMethod { get; set; }

        [ConditionalMaximumWeight("Carrier", "POS", 40)]
        [Required(ErrorMessage = "Weight required")]
        public int? Weight { get; set; }

        [Required(ErrorMessage = "Zip code required")]
        public string ZipCode { get; set; }

        public decimal Surcharge { get; set; }

        public ShipRate() {
            Carriers = new List<SelectListItem>();
            ShipMethods = new List<SelectListItem>();
        }
    }

    public class ConditionalMaximumWeightAttribute : ValidationAttribute, IClientValidatable {
        private const string ERRORMSG = "Weight must not exceed {0} lbs.";

        public string DependentProperty { get; set; }

        public string DependentValue { get; set; }

        public int MaximumWeight { get; set; }

        public ConditionalMaximumWeightAttribute(string dependentProperty, string dependentValue, int maximumWeight) {
            this.DependentProperty = dependentProperty;
            this.DependentValue = dependentValue;
            this.MaximumWeight = maximumWeight;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) {
            var rule = new ModelClientValidationRule() {
                ErrorMessage = String.Format(ERRORMSG, this.MaximumWeight),
                ValidationType = "maximumweight",
            };

            string depProp = BuildDependentPropertyId(metadata, context as ViewContext);

            rule.ValidationParameters.Add("dependentproperty", depProp);
            rule.ValidationParameters.Add("dependentvalue", this.DependentValue);
            rule.ValidationParameters.Add("weightvalue", this.MaximumWeight);

            yield return rule;
        }

        /// <summary>
        /// Only MOSTLY stolen from http://blogs.msdn.com/b/simonince/archive/2011/02/04/conditional-validation-in-asp-net-mvc-3.aspx
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            // get a reference to the property this validation depends upon
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(this.DependentProperty);

            if (field != null) {
                // get the value of the dependent property
                var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

                var weight = containerType.GetProperty(validationContext.DisplayName);
                int weightvalue = (int)weight.GetValue(validationContext.ObjectInstance, null);

                // compare the value against the target value
                if (dependentvalue.ToString() == this.DependentValue && weightvalue > this.MaximumWeight) {
                    // validation failed - return an error
                    return new ValidationResult(String.Format(ERRORMSG, this.MaximumWeight));
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Straight up stolen from http://blogs.msdn.com/b/simonince/archive/2011/02/04/conditional-validation-in-asp-net-mvc-3.aspx
        /// </summary>
        private string BuildDependentPropertyId(ModelMetadata metadata, ViewContext viewContext) {
            // build the ID of the property
            string depProp = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(this.DependentProperty);
            // unfortunately this will have the name of the current field appended to the beginning,
            // because the TemplateInfo's context has had this fieldname appended to it. Instead, we
            // want to get the context as though it was one level higher (i.e. outside the current property,
            // which is the containing object (our Person), and hence the same level as the dependent property.
            var thisField = metadata.PropertyName + "_";
            if (depProp.StartsWith(thisField))
                // strip it off again
                depProp = depProp.Substring(thisField.Length);
            return depProp;
        }
    }

    public class ShipRateDetail {

        public int Weight { get; set; }

        public string ZipCode { get; set; }

        public int NumberOfBoxes { get; set; }

        public double TotalCost { get; set; }
    }

    public class ShipRateUpdate {

        [Required]
        public string Carrier { get; set; }

        public bool Header { get; set; }

        [Required]
        public HttpPostedFileBase PostedFile { get; set; }
    }

    public class ShipRatePOS {

        public int Weight { get; set; }

        public decimal Price { get; set; }
    }

    public class ShipRateUPS {

        public int Weight { get; set; }

        public decimal Price { get; set; }

        public int Zone { get; set; }

        public string Method { get; set; }
    }
}