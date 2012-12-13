MVC Custom Attribute
====================
This is a fully-functional demo site to highlight the use of a custom validation attribute. The attribute is a conditional validator that  uses another property's value to validate against if a certain conditon is met. There is also jQuery validation as well as unobtrusive validation.

How do I use it?
----------------
The easiest way to check it out is to load it up in Visual Studio and press F5.

What does it do?
----------------
The demo site is a shipping rate calculator (which is why this validator was made). You select a shipping type, and then anotehr drop-down is dynamically populated - so there is a litlle extra goodness to check out as well. Then, based on values entered a total shipping cost is calculated.

The custom validation comes in to play based on your shipping type. If the shipping type chosen is USPS, then we only want to ship packages that weight less than a certain amount (35 lbs in this case, but that can be changed). Our shipping department saves money by using UPS rather than USPS for heavier packages, hence the conditional validation requirement.

What is used?
-------------
Really, the main important bit is the conditional validator, but this is a full-blown site demo. It includes:

* Twitter Bootstrap for "prettification"
* jQuery
* jQuery validation (including unobtrusive)

Other cool stuff
----------------
As I mentioned above, there is the use of a dynamically populated drop-down list - that is in an HTML helper. There is also some basic data access code as well as some borrowed code from Javier Lozano:
http://lozanotek.com/blog/archive/2007/05/09/Converting_Custom_Collections_To_and_From_DataTable.aspx

This is an older demo that I just cleaned up, or else I'd have just went with Massive for the data access. But, it is kinda nice to kick it old school, so to speak.

History
-------
**20121213** - Initial release