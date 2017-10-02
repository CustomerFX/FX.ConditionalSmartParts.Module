# FX.ConditionalSmartParts.Module
## A configurable module for Infor CRM that allows you to specify SmartParts to show or hide based on values of an entity property

This module allows you to easily show or hide SmartParts based on values of the entity. For example, if you wanted to show certain SmartParts only for Account records with a Type value of "Customer" and other SmartParts only for Account records with a value of "Vendor". This can be easily configured in this module's config file and then it all magically works at runtime.

### Usage Instructions

To use this module, simply perform the following steps:
1. Install the provided bundle [FX.ConditionalSmartParts.Module.zip](https://github.com/CustomerFX/FX.ConditionalSmartParts.Module/raw/master/Deliverables/FX.ConditionalSmartParts.Module.zip)
2. Open the SlxClient portal in Application Arhitect and select the SupportFiles tab
3. Locate the FX.ConditionalSmartParts.json file and double-click to open it
4. Add an entry for the entity, the property to check, and the SmartParts that should show for each value
5. Deploy

The way this works is if a SmartPart is *not* listed for a value in the config file, the SmartPart will show for all records. If a SmartPart is listed for a value in the config file, it will only show for records with that value. You can use this for several different entities or even for moltiple different fields for the same entity.

### Sample Configuration File

The following is a sample configuration file:

```json
{
  "Active": true,
  "ConfigEntities": [
    {
      "Entity": "Account",
      "EntityProperty": "Address.City",
      "ConfigValues": [
        {
          "Value": "Chicago",
          "SmartParts": [
            "AccountContacts",
            "AccountDetails"
          ]
        },
        {
          "Value": "Phoenix",
          "SmartParts": [
            "AccountOpportunities"
          ]
        }
      ]
    }
  ]
}
```

The `Active` property allows you to turn on/off this module with one setting. 

Note: The `EntityProperty` specifies a property for the current entity (specified in the `Entity` property). This can be a simple or complex property. For example, for the Account entity, this can be a simple property, such as `Type`, or a complex property, such as `Address.City`.

No warranty, guarantees, or support is provided. Pull requests and [issue reports](https://github.com/CustomerFX/FX.ConditionalSmartParts.Module/issues) are accepted.

Copyright (C) 2017 Customer FX Corporation  
[customerfx.com](http://customerfx.com)
