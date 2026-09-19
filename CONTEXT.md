# Billow

Billing software for a single Indian retail business, producing GST-compliant documents for its sales.

## Language

### The business

**Business**:
The one business that this Billow install bills for. GST law calls it the "supplier".
_Avoid_: Store, shop, company, seller, supplier ("supplier" is kept for whoever the Business buys stock from)

**Legal Name**:
The Business's name as it appears on its PAN and GST registration.
_Avoid_: Registered name, company name

**Trade Name**:
The name the Business trades under, usually the one on its signboard. It may differ from the Legal Name.
_Avoid_: Shop name, brand name, display name

**Registration Type**:
The Business's GST status. It is one of **Regular**, **Composition** or **Unregistered**, and it decides which kind of document the Business may issue and whether it can charge GST.
_Avoid_: GST type, dealer type, tax scheme

**Regular**:
A Registration Type for a business that has a GSTIN, charges GST and issues Tax Invoices.

**Composition**:
A Registration Type for a business that has a GSTIN but pays tax under the composition scheme. It cannot charge GST to customers and issues Bills of Supply.

**Unregistered**:
A Registration Type for a business without a GSTIN. It cannot charge GST.

**Additional Registration**:
A licence or registration number, other than the GSTIN, that the Business must or chooses to print on its documents. Examples are an FSSAI licence, a Drug Licence or a Udyam number. Each has a label and a number.
_Avoid_: Licence, extra ID

### Documents

**Bill**:
The document the Business issues to a customer for a sale. Depending on the Registration Type, it is a Tax Invoice, a Bill of Supply, or (for an Unregistered Business) a plain Bill. Bills are only ever for sales.
_Avoid_: Invoice (as the general term), sale, receipt, purchase bill

**Tax Invoice**:
The kind of Bill a Regular Business issues, showing the GST it charged.

**Bill of Supply**:
The kind of Bill a Composition Business issues. It carries no GST, only the composition declaration.

### Buying stock

**Supplier**:
A party the Business buys stock from.
_Avoid_: Vendor, party, dealer

**Purchase**:
The Business's record of stock bought from a Supplier.
_Avoid_: Purchase bill, GRN, inward

**Supplier Invoice**:
The document a Supplier issues to the Business for a Purchase.
_Avoid_: Purchase bill, bill (on the buying side)
