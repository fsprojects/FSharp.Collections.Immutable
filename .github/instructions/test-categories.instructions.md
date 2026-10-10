---
applyTo: "tests/**/*.fs"
---

# Test category attributes

* Do not write the category as a string in `[<TestCategory "...">]`.
* Make each attribute give one category. Do not put two categories into one attribute.
* Use the `TestCategoryAttribute` suffix in the name of each attribute.
* The category is the name without the suffix, written as separate words. Start the category with a capital letter. Write the other words in lower case.
* Keep the spelling of a product name or a type name in a category.
* Put the attribute of the operation or component that the class examines on the class.
* Keep the attributes of each `TestCategories.fs` file sorted alphabetically by type name. Put a new attribute at its sorted position. Do not put a new attribute at the end of the file.
