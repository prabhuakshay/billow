# Billow

Billing software for a single Indian retail business, producing GST-compliant documents for its sales.

## Language

### The business

**Business**:
The one business that this Billow install bills for. GST law calls it the "supplier".
_Avoid_: Store, shop, company, seller, supplier ("supplier" is kept for whoever the Business buys stock from)

**Legal Name**:
A party's name as it appears on its PAN and GST registration. Both the Business and a B2B Customer have one.
_Avoid_: Registered name, company name

**Trade Name**:
The name a party trades under, usually the one on its signboard. It may differ from the Legal Name. The Business and any Customer may have one.
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

### Selling

**Customer**:
A party the Business sells to and keeps on record so it can bill them again. A sale does not need a Customer: a walk-in buyer who gives no details gets a Bill with no Customer on it.
_Avoid_: Party, buyer, client, recipient, debtor

**B2B Customer**:
A Customer with a GSTIN. Its Legal Name, address, State and PIN are all required, and its State is always the one its GSTIN belongs to. A Customer on the composition scheme is still a B2B Customer.
_Avoid_: Registered party, dealer

**B2C Customer**:
A Customer without a GSTIN. Only a name is required.
_Avoid_: Retail customer, consumer

**Walk-in Sale**:
A sale billed with no Customer, to a buyer whose details are not recorded.
_Avoid_: Cash sale, counter sale

**Bill To**:
The Customer details printed on a Bill as the party being billed.

**Ship To**:
The address the goods are delivered to, printed on a Bill when it differs from the Bill To address. It belongs to the Bill, not the Customer, and applies to B2B and B2C sales alike.
_Avoid_: Delivery address, consignee

**Place of Supply**:
The State where a sale is treated as taking place: for goods, where delivery ends. Comparing it with the Business's State decides whether a Regular Business charges IGST or CGST and SGST, and a Composition Business may only sell within its own State.

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
