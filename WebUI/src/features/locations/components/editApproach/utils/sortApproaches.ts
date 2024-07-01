import { ApproachForConfig } from '../../editLocation/editLocationConfigHandler'

export const sortApproachesByPhaseNumber = (
  approaches: ApproachForConfig[]
) => {
  return [...approaches].sort((a, b) => {
    // Extract the digit from the description, if present
    const aNum = a.description.match(/\d+/) // Match all digits to handle multi-digit numbers
    const bNum = b.description.match(/\d+/)

    // Handle cases where the description might not contain any digits
    if (!aNum && !bNum) return 0 // Both are without numbers, maintain order
    if (!aNum) return -1 // a without a number, should come first
    if (!bNum) return 1 // b without a number, should come first

    // Compare by numerical value if both have numbers
    return Number(aNum[0]) - Number(bNum[0])
  })
}
