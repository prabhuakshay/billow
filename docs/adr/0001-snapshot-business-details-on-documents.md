# Snapshot Business details onto each issued document

Each document Billow issues keeps its own copy of the Business details it printed: Legal Name, Trade Name, address, GSTIN, Registration Type and Additional Registrations. It does not look them up from the current Business record. A GST document has to reprint exactly as it was issued, even after the Business moves, changes its GSTIN or switches between Composition and Regular.

The same applies to the Customer: a Bill keeps its own copy of the Customer's name, address, State and GSTIN, so that editing or removing a Customer never changes a Bill already issued. Snapshotting does this with no extra lookup logic, at the small cost of repeating some text on every document.

## Considered Options

- **Dated versions of the Business details**, with each document pointing at one version. Rejected: it gives the same result with more moving parts, and editing an old version by mistake would silently change past documents.
- **Always read the current details.** Rejected: reprints of old documents would show details that were wrong when those documents were issued.
